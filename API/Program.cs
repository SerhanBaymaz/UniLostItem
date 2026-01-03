using API.Extensions;
using API.Helpers;
using API.Middleware;
using Infrastructure;
using Serilog;
using DotNetEnv;

// Load environment variables from .env files
EnvLoader.Load();

#region Builder and Configuration
var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddEnvironmentConfiguration(builder.Environment);
builder.Host.AddSerilogConfiguration();

// Add services
builder.Services.AddApiConfiguration();
builder.Services.AddRateLimitingServices(builder.Configuration);
builder.Services.AddHealthCheckServices(builder.Configuration);
builder.Services.AddDatabaseServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddInfrastructureServices();
builder.Services.AddApplicationServices();
builder.Services.AddCorsConfiguration();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddTransient<ExceptionMiddleware>();

var app = builder.Build();
#endregion

#region HTTP request pipeline
// Configure middleware pipeline
app.UseSerilogRequestLogging();
app.UseMiddleware<ExceptionMiddleware>();

//app.UseHttpsRedirection();
//app.UseCookiePolicy();
//app.UseRouting();
app.UseRateLimiter();
app.UseCorsConfiguration();
app.UseHealthChecksConfiguration();

app.UseAuthentication();
app.UseAuthorization();

app.UseSwaggerDocumentation(app.Environment);
app.MapControllers();
#endregion


await app.ApplyMigrationsAndSeed();

await app.RunAsync();
await Log.CloseAndFlushAsync();
