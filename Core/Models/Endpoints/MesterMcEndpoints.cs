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

#if DEBUG
    public const string AuthEndpoint = "http://localhost:36767/launcher/login";
    
    public const string TfaEndpoint = "http://localhost:36767/launcher/tfa";
    
    public const string ApiBaseEndpoint = "http://localhost:36767/";
#else
    public const string AuthEndpoint = "https://api.mestermc.hu/launcher/login";
    
    public const string TfaEndpoint = "https://api.mestermc.hu/launcher/tfa";
    
    public const string ApiBaseEndpoint = "https://api.mestermc.hu/";
#endif
}