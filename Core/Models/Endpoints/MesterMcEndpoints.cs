namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

/// <summary>
/// Provides endpoint URLs for accessing MMC-related resources.
/// </summary>
public static class MesterMcEndpoints
{
    /// <summary>
    /// The URL for retrieving the latest release of Konkord.
    /// </summary>
    public const string LatestRelease = "https://api.github.com/repos/TavstalDev/MMC-Launcher/releases/latest";

    /// <summary>
    /// The URL for retrieving all releases of Konkord.
    /// </summary>
    public const string AllReleases = "https://api.github.com/repos/TavstalDev/MMC-Launcher/releases";
    
    public const string AuthEndpoint = "https://api.mestermc.hu/auth/login";
    
    public const string TfaEndpoint = "https://api.mestermc.hu/auth/2fa";
    
    public const string ApiBaseEndpoint = "https://api.mestermc.hu/";
}