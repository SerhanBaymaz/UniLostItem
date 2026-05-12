using DotNetEnv;

namespace API.Extensions;

public static class ConfigurationExtensions
{
    public static void AddEnvironmentConfiguration(this IConfigurationBuilder configurationBuilder, IWebHostEnvironment environment)
    {
        // Env variables are already loaded by EnvLoader in Program.cs : API/EnvLoader.cs

        // Override configuration with environment variables
        var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
        var seqServerUrl = Environment.GetEnvironmentVariable("SeqServerUrl");
        var cloudinaryCloudName = Environment.GetEnvironmentVariable("Cloudinary__CloudName");
        var cloudinaryApiKey = Environment.GetEnvironmentVariable("Cloudinary__ApiKey");
        var cloudinaryApiSecret = Environment.GetEnvironmentVariable("Cloudinary__ApiSecret");

        if (!string.IsNullOrEmpty(connectionString) && !string.IsNullOrEmpty(seqServerUrl))
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Serilog:WriteTo:1:Args:serverUrl"] = seqServerUrl,
                ["Cloudinary:CloudName"] = cloudinaryCloudName,
                ["Cloudinary:ApiKey"] = cloudinaryApiKey,
                ["Cloudinary:ApiSecret"] = cloudinaryApiSecret
            }!);
        }
    }
}
