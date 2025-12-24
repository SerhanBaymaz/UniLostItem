using API.Middleware;
using Application.Core;
using FluentValidation;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using API.Responses;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;
using System.Diagnostics;
using Application.Features.SerhanKitaplar.Validators;
using API.Helpers;
using DotNetEnv;

#region Builder and Configuration

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from .env file based on environment (only when not in Docker)
var envFile = builder.Environment.IsDevelopment() ? "../dev.env" : "../prod.env";
if (File.Exists(envFile))
{
    Env.Load(envFile);
}

// Override configuration with environment variables
var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
var seqServerUrl = Environment.GetEnvironmentVariable("SeqServerUrl");

if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(seqServerUrl))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["ConnectionStrings:DefaultConnection"] = connectionString,
        ["Serilog:WriteTo:1:Args:serverUrl"] = seqServerUrl
    }!);
}

builder.Host.UseSerilog((context, loggerConfig) =>
{
    loggerConfig.ReadFrom.Configuration(context.Configuration);
});

// Add services to the container.
builder.Services.AddControllers();

// Convert model state / input formatter errors into StandardApiResponse
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    // disable the default automatic 400 so our factory runs
    options.SuppressModelStateInvalidFilter = true;
    options.InvalidModelStateResponseFactory = ModelStateResponseFactory.Create;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Uni Lost Item API", Version = "v1" });
    // Use full type names for schema Ids to avoid collisions from nested types
    c.CustomSchemaIds(type => type.FullName?.Replace('+', '.') ?? type.Name);
});
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
builder.Services.AddCors();
builder.Services.AddMediatR(x =>
{
    x.RegisterServicesFromAssemblyContaining<MappingProfiles>();
    x.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
builder.Services.AddValidatorsFromAssemblyContaining<CreateSerhanKitapCommandValidator>();
builder.Services.AddTransient<ExceptionMiddleware>();

var app = builder.Build();

#endregion

#region HTTP request pipeline
app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        // Prefer Activity.TraceId (distributed tracing) when available
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;
        diagnosticContext.Set("TraceId", traceId);
        // Also include the W3C traceparent (includes version/trace/span/flags) so it matches API responses
        var traceParent = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        diagnosticContext.Set("TraceParent", traceParent);
    });
app.UseMiddleware<ExceptionMiddleware>();

//app.UseHttpsRedirection();
//app.UseCookiePolicy();
//app.UseRouting();
//app.UseRateLimiter();
app.UseCors(x => x.AllowAnyHeader().AllowAnyMethod()
    .WithOrigins("http://localhost:3000", "https://localhost:3000"));


//app.UseAuthentication();
//app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Uni Lost Item API v1");
    });
}

app.MapControllers();

#endregion

#region  Run Migrations and Seed Data
// Apply migrations and seed database
using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedData(context);
}
catch (Exception ex)
{
    Log.Fatal(ex, "An error occurred during migration.");
}
#endregion

await app.RunAsync();
await Log.CloseAndFlushAsync();
