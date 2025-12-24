using DotNetEnv;

namespace API.Extensions;

public static class ConfigurationExtensions
{
    public static void AddEnvironmentConfiguration(this IConfigurationBuilder configurationBuilder, IWebHostEnvironment environment)
    {
        // Load environment variables from .env file based on environment (only when not in Docker)
        var envFile = environment.IsDevelopment() ? "../dev.env" : "../prod.env";
        if (File.Exists(envFile))
        {
            Env.Load(envFile);
        }

        // Override configuration with environment variables
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
        var seqServerUrl = Environment.GetEnvironmentVariable("SeqServerUrl");

        if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(seqServerUrl))
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Serilog:WriteTo:1:Args:serverUrl"] = seqServerUrl
            }!);
        }
    }
}
