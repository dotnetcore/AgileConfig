using Microsoft.Extensions.Configuration;

namespace AgileConfig.Server.Data.MongodbTests;

internal static class TestConfiguration
{
    private const string DefaultAppSettingsRelativePath = "src/AgileConfig.Server.Apisite/appsettings.json";

    /// <summary>
    ///     Loads the real appsettings.json shipped with the API site, including its commented defaults,
    ///     so tests can prove that the default configuration does not change the driver behavior.
    /// </summary>
    public static IConfiguration LoadDefaultAppSettings()
    {
        return new ConfigurationBuilder()
            .AddJsonFile(FindRepositoryFile(DefaultAppSettingsRelativePath), optional: false, reloadOnChange: false)
            .Build();
    }

    private static string FindRepositoryFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }

        Assert.Fail($"Could not locate '{relativePath}' from '{AppContext.BaseDirectory}'.");
        return null;
    }
}
