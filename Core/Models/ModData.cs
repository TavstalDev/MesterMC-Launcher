namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Represents metadata for a single mod (read-only after construction).
/// Used to hold identifying information and download/hash data for a mod.
/// </summary>
public class ModData
{
    /// <summary>
    /// The file name of the mod.
    /// </summary>
    public string Name { get;}

    /// <summary>
    /// The expected SHA-256 hash of the mod file (hex string).
    /// Used to verify integrity after download.
    /// </summary>
    public string Sha256Hash { get; }

    /// <summary>
    /// The URL where the mod binary can be downloaded.
    /// </summary>
    public string Url { get; }

    /// <summary>
    /// Indicates whether the mod is disabled (should not be loaded/applied).
    /// </summary>
    public bool IsDisabled { get; }
    
    /// <summary>
    /// Creates a new <see cref="ModData"/> instance with the mod enabled by default.
    /// </summary>
    /// <param name="name">Mod file name.</param>
    /// <param name="sha256Hash">Expected SHA-256 hash (hex string).</param>
    /// <param name="url">Download URL for the mod.</param>
    public ModData(string name, string sha256Hash, string url)
    {
        Name = name;
        Sha256Hash = sha256Hash;
        Url = url;
        IsDisabled = false;
    }
    
    /// <summary>
    /// Creates a new <see cref="ModData"/> instance with an explicit disabled flag.
    /// </summary>
    /// <param name="name">Mod file name.</param>
    /// <param name="sha256Hash">Expected SHA-256 hash (hex string).</param>
    /// <param name="url">Download URL for the mod.</param>
    /// <param name="isDisabled">Whether the mod should be considered disabled.</param>
    public ModData(string name, string sha256Hash, string url, bool isDisabled)
    {
        Name = name;
        Sha256Hash = sha256Hash;
        Url = url;
        IsDisabled = isDisabled;
    }
}