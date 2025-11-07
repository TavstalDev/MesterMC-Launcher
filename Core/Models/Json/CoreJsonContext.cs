using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Core.Models.Fabric;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;

namespace Tavstal.KonkordLauncher.Core.Models.Json;

[JsonSerializable(typeof(VersionManifest))]
[JsonSerializable(typeof(FabricVersionMeta))]
[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyFields = true, IgnoreReadOnlyProperties = true)]
public partial class CoreJsonContext : JsonSerializerContext
{
    
}