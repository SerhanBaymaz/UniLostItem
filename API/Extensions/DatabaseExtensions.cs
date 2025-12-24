using Microsoft.EntityFrameworkCore;
using Persistence;
using Serilog;

namespace API.Extensions;

public static class DatabaseExtensions
{
    public static void AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());
    }

    public static async Task ApplyMigrationsAndSeed(this WebApplication app)
    {
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
    }
}
