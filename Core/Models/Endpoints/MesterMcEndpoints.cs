namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

/// <summary>
/// Provides endpoint URLs for accessing MMC-related resources.
/// </summary>
public static class MesterMcEndpoints
{
    /// <summary>
    /// The URL for retrieving the latest release
    /// </summary>
    public const string LatestRelease = "https://api.github.com/repos/TavstalDev/MesterMC-Launcher/releases/latest";

    /// <summary>
    /// The URL for retrieving all releases
    /// </summary>
    public const string AllReleases = "https://api.github.com/repos/TavstalDev/MesterMC-Launcher/releases";

    public static string BaseEndpoint { get; set; } = 
#if DEBUG
        "https://localhost:36767";
#else
        Constants.ApiUrl;
#endif
    
    public static string AuthEndpoint => $"{BaseEndpoint}/login/launcher";
    public static string TfaEndpoint => $"{BaseEndpoint}/login/launcher/2fa";
    public static string YggdrasilEndpoint => $"{BaseEndpoint}/yggdrasil";
    public static string ApiBaseEndpoint => $"{BaseEndpoint}/";
}