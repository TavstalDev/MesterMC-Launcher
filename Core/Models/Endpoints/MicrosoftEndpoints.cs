namespace Tavstal.KonkordLauncher.Core.Models.Endpoints;

/// <summary>
/// Provides endpoint URLs and helper methods for Microsoft, Xbox, and Minecraft authentication.
/// </summary>
public static class MicrosoftEndpoints
{
    /// <summary>
    /// The URL for retrieving the Minecraft version manifest.
    /// </summary>
    public const string MinecraftManifestUrl = "https://piston-meta.mojang.com/mc/game/version_manifest_v2.json";
    
    /// <summary>
    /// The base URL for downloading Minecraft resources.
    /// </summary>
    public const string MinecraftResourcesUrl = "https://resources.download.minecraft.net";
}