using System.Collections.Generic;
using System.Text.Json.Serialization;
using Tavstal.MesterMC.Launcher.Models.Config.DTOs;
using Tavstal.MesterMC.Launcher.Models.Config.Java;

namespace Tavstal.MesterMC.Launcher.Models.Json;

/// <summary>
/// Represents a custom JSON serialization context for the application, 
/// specifying the types to be serialized and the serialization options.
/// </summary>
[JsonSerializable(typeof(CoreConfigDto))] // Specifies that the CoreConfigDto type is serializable.
[JsonSerializable(typeof(JavaMirrorConfig))] // Specifies that the JavaMirrorConfig type is serializable.
[JsonSerializable(typeof(JavaMirrorJdks))] // Specifies that the JavaMirrorJdks type is serializable.
[JsonSerializable(typeof(List<NewsDto>))] // Specifies that a list of NewsDto objects is serializable.
[JsonSourceGenerationOptions(
    WriteIndented = true, // Enables indented (pretty-printed) JSON output.
    IgnoreReadOnlyFields = true, // Ignores read-only fields during serialization.
    IgnoreReadOnlyProperties = true // Ignores read-only properties during serialization.
)]
public partial class CustomJsonContext : JsonSerializerContext;