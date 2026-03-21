using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Tavstal.KonkordLauncher.Core.Models.Fabric;

/// <summary>
/// Metadata for a Fabric version JSON file. Mirrors the structure used by Fabric installer/loader metadata.
/// </summary>
public class FabricVersionMeta
{
    /// <summary>
    /// Launch arguments meta-information (game and JVM arguments).
    /// Mapped from JSON property "arguments".
    /// </summary>
    [JsonPropertyName("arguments"), JsonProperty("arguments")]
    public ArgumentMeta Arguments { get; set; }
    
    /// <summary>
    /// The version identifier (e.g., "1.16.5+build.23").
    /// Mapped from JSON property "id".
    /// </summary>
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id { get; set; }
    
    /// <summary>
    /// If present, the ID of a parent version this file inherits from (usually the Mojang version).
    /// Mapped from JSON property "inheritsFrom".
    /// </summary>
    [JsonPropertyName("inheritsFrom"), JsonProperty("inheritsFrom")]
    public string InheritsFrom { get; set; }
    
    /// <summary>
    /// List of libraries required by this Fabric version (includes Fabric libraries and dependencies).
    /// Mapped from JSON property "libraries".
    /// </summary>
    [JsonPropertyName("libraries"), JsonProperty("libraries")]
    public List<FabricLibrary> Libraries { get; set; }
    
    /// <summary>
    /// Fully-qualified main class to launch (typically provided by Fabric loader or inherited version).
    /// Mapped from JSON property "mainClass".
    /// </summary>
    [JsonPropertyName("mainClass"), JsonProperty("mainClass")]
    public string MainClass { get; set; }
    
    /// <summary>
    /// The type of this version metadata (e.g., "release", "snapshot", or loader-specific type).
    /// Mapped from JSON property "type".
    /// </summary>
    [JsonPropertyName("type"), JsonProperty("type")]
    public string Type { get; set; }
}
