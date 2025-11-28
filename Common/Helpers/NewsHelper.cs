using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Provides helper methods for fetching news data from the server.
/// </summary>
public static class NewsHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(NewsHelper));

    /// <summary>
    /// Asynchronously fetches news data from the server.
    /// </summary>
    /// <remarks>
    /// This method sends a GET request to the news endpoint and deserializes the response into a list of <see cref="NewsData"/> objects.
    /// If an error occurs during the operation, it is logged, and the method returns null.
    /// </remarks>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of <see cref="NewsData"/> objects
    /// or null if the operation fails.
    /// </returns>
    /// <exception cref="RequiresUnreferencedCodeAttribute">
    /// Indicates that this method uses code that may be removed during trimming.
    /// </exception>
    [RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
    public static async Task<List<NewsData>?> FetchNewsAsync()
    {
        try
        {
            // Construct the endpoint URL for fetching news
            var endpoint = new Uri(new (MesterMcEndpoints.ApiBaseEndpoint), "news").ToString();

            // Fetch the raw JSON response from the server
            string? rawJson = await HttpHelper.GetStringAsync(endpoint);
            if (rawJson == null)
                return null;

            // Deserialize the JSON response into a list of NewsData objects
            return JsonSerializer.Deserialize<List<NewsData>>(rawJson);
        }
        catch (Exception ex)
        {
            // Log any errors that occur during the operation
            _logger.Error("Failed to fetch news: " + ex.Message);
            return null;
        }
    }
}