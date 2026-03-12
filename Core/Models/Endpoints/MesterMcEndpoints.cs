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

#if DEBUG
    public const string AuthEndpoint = "https://localhost:36767/login/launcher";
    
    public const string TfaEndpoint = "https://localhost:36767/login/launcher/2fa";
    
    public const string YggdrasilEndpoint = "https://localhost:36767/yggdrasil";
    
    public const string ApiBaseEndpoint = "https://localhost:36767/";
#else
    public const string AuthEndpoint = "https://api.mestermc.hu/login/launcher";
    
    public const string TfaEndpoint = "https://api.mestermc.hu/login/launcher/2fa";

    public const string YggdrasilEndpoint = "https://api.mestermc.hu/yggdrasil";
    
    public const string ApiBaseEndpoint = "https://api.mestermc.hu/";
#endif
}