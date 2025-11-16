using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

namespace Tavstal.KonkordLauncher.Common.Helpers;

public static class NewsHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(NewsHelper));

    [RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
    public static async Task<List<NewsData>?> FetchNewsAsync()
    {
        try
        {
            var endpoint = new Uri(new (MesterMcEndpoints.ApiBaseEndpoint), "news").ToString();
            string? rawJson = await HttpHelper.GetStringAsync(endpoint);
            if (rawJson == null)
                return null;
            return JsonSerializer.Deserialize<List<NewsData>>(rawJson);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to fetch news: " + ex.Message);
            return null;
        }
    }
}