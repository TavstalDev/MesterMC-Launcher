using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.Fabric;

/// <summary>
/// Represents a Fabric library entry from a Fabric version manifest.
/// Contains metadata (name, checksums, size) and helpers to compute the download URL and relative path.
/// </summary>
public class FabricLibrary
{
    /// <summary>
    /// The Maven coordinate in the form "group:artifact:version".
    /// </summary>
    [JsonPropertyName("name"), JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Base URL for this library (may be null/empty if not provided in manifest).
    /// The full download URL is computed by combining this with the path derived from <see cref="Name"/>.
    /// </summary>
    [JsonPropertyName("url"), JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// MD5 checksum (hex) for the library file, if present.
    /// Used for integrity checks after download.
    /// </summary>
    [JsonPropertyName("md5"), JsonProperty("md5")]
    public string Md5 { get; set; }

    /// <summary>
    /// SHA-1 checksum (hex) for the library file, if present.
    /// Used for integrity checks after download.
    /// </summary>
    [JsonPropertyName("sha1"), JsonProperty("sha1")]
    public string Sha1 { get; set; }

    /// <summary>
    /// SHA-256 checksum (hex) for the library file, if present.
    /// Used for stronger integrity checks after download.
    /// </summary>
    [JsonPropertyName("sha256"), JsonProperty("sha256")]
    public string Sha256 { get; set; }

    /// <summary>
    /// SHA-512 checksum (hex) for the library file, if present.
    /// </summary>
    [JsonPropertyName("sha512"), JsonProperty("sha512")]
    public string Sha512 { get; set; }

    /// <summary>
    /// Size of the library file in bytes as reported in the manifest.
    /// </summary>
    [JsonPropertyName("size"), JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// Default constructor required for JSON deserialization.
    /// </summary>
    public FabricLibrary() { }

    /// <summary>
    /// Builds the full download URL for this library by converting the Maven coordinate to a path
    /// and appending it to <see cref="Url"/>.
    /// Example: group "org.example", artifact "lib", version "1.0" => {Url}/org/example/lib/1.0/lib-1.0.jar
    /// </summary>
    /// <returns>The full URL to download the library JAR.</returns>
    public string GetURL()
    {
        string[] parts = Name.Split(":", 3);
        var path = parts[0].Replace(".", "/") + "/" + parts[1] + "/" + parts[2] + "/" + parts[1] + "-" + parts[2] + ".jar";

        return Url + path;
    }

    /// <summary>
    /// Computes a relative filesystem path for the library JAR based on the Maven coordinate.
    /// Spaces in the resulting path are replaced with underscores.
    /// Example result: "org/example/lib/1.0/lib-1.0.jar"
    /// </summary>
    /// <returns>Relative path (slash-separated) for the library JAR, safe for filesystem use.</returns>
    public string GetPath()
    {
        string[] parts = Name.Split(":", 3);
        char separator = '/';
        string path = parts[0].Replace('.', separator) + separator + parts[1] + separator + parts[2] + separator + parts[1] + "-" + parts[2] + ".jar";
        return path.Replace(" ", "_");
    }
}