using System.Text.Json.Serialization;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.Java;

namespace Tavstal.KonkordLauncher.Common.Models.Json;

[JsonSerializable(typeof(CoreConfig))]
[JsonSerializable(typeof(JavaMirrorConfig))]
[JsonSerializable(typeof(List<NewsData>))]
[JsonSourceGenerationOptions(WriteIndented = true, IgnoreReadOnlyFields = true, IgnoreReadOnlyProperties = true)]
public partial class CommonJsonContext : JsonSerializerContext
{
    
}