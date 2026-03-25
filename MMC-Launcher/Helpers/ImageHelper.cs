using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.MesterMC.Launcher.Helpers;

/// <summary>
/// Provides helper methods for loading images from various sources, such as local resources or web URLs.
/// </summary>
public static class ImageHelper
{
    /// <summary>
    /// Loads an image from the specified file path or URI.
    /// </summary>
    /// <param name="path">The file path or URI of the image to load.</param>
    /// <returns>A <see cref="Bitmap"/> object if the image is successfully loaded; otherwise, null.</returns>
    public static async Task<Bitmap?> LoadAsync(string path) => await LoadAsync(new Uri(path));

    /// <summary>
    /// Loads an image from the specified URI, either from the web or a local resource.
    /// </summary>
    /// <param name="resourceUri">The URI of the image to load.</param>
    /// <returns>A <see cref="Bitmap"/> object if the image is successfully loaded; otherwise, null.</returns>
    public static async Task<Bitmap?> LoadAsync(Uri resourceUri)
    {
        if (resourceUri.ToString().StartsWith("http"))
            return await LoadFromWeb(resourceUri) ?? null;
        return LoadFromResource(resourceUri);
    }

    /// <summary>
    /// Loads an image from a local resource.
    /// </summary>
    /// <param name="resourceUri">The URI of the local resource to load.</param>
    /// <returns>A <see cref="Bitmap"/> object representing the loaded image.</returns>
    public static Bitmap LoadFromResource(Uri resourceUri)
    {
        return new Bitmap(AssetLoader.Open(resourceUri));
    }

    /// <summary>
    /// Loads an image from a web URL.
    /// </summary>
    /// <param name="url">The URL of the image to load.</param>
    /// <param name="logger">Optional custom logger</param>
    /// <returns>A <see cref="Bitmap"/> object if the image is successfully downloaded and loaded; otherwise, null.</returns>
    public static async Task<Bitmap?> LoadFromWeb(Uri url, CoreLogger? logger = null)
    {
        using var httpClient = HttpHelper.GetHttpClient();
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return new Bitmap(new MemoryStream(data));
        }
        catch (HttpRequestException ex)
        {
            if (logger != null)
                logger.Error($"An error occurred while downloading image '{url}' : {ex}");
            else
                Console.WriteLine($"An error occurred while downloading image '{url}' : {ex}");
            return null;
        }
    }
}