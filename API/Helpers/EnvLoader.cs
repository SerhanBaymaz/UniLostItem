using DotNetEnv;

namespace API.Helpers;

public static class EnvLoader
{
    public static void Load()
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var envFileName = environmentName == "Development" ? "dev.env" : "prod.env";
        var currentDir = Directory.GetCurrentDirectory();

        var envPath = Path.Combine(currentDir, envFileName);

        // Try to find in parent directory if not in current
        if (!File.Exists(envPath))
        {
            envPath = Path.Combine(currentDir, "..", envFileName);
        }

        // Fallback to example files
        if (!File.Exists(envPath))
        {
            var exampleEnvName = environmentName == "Development" ? "example.dev.env" : "example.prod.env";
            var examplePath = Path.Combine(currentDir, "..", exampleEnvName);
            if (File.Exists(examplePath))
            {
                envPath = examplePath;
            }

        }

        if (File.Exists(envPath))
        {
            Env.Load(envPath);
            Console.WriteLine($"Loaded environment variables from: {envPath}");
        }
    }
}
