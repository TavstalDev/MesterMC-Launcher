using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.MesterMC.Launcher.Models;

/// <summary>
/// Represents a data transfer object (DTO) for news items, containing the title, content, and banner URL.
/// </summary>
public class NewsDto
{
    /// <summary>
    /// Gets or sets the title of the news item.
    /// </summary>
    /// <remarks>
    /// This property is serialized to and from JSON using the "Title" key.
    /// </remarks>
    [JsonPropertyName("Title"), JsonProperty("Title")]
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the content of the news item.
    /// </summary>
    /// <remarks>
    /// This property is serialized to and from JSON using the "Content" key.
    /// </remarks>
    [JsonPropertyName("Content"), JsonProperty("Content")]
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the URL of the banner image associated with the news item.
    /// </summary>
    /// <remarks>
    /// This property is serialized to and from JSON using the "BannerUrl" key.
    /// </remarks>
    [JsonPropertyName("BannerUrl"), JsonProperty("BannerUrl")]
    public string BannerUrl { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsDto"/> class with the specified title, content, and banner URL.
    /// </summary>
    /// <param name="title">The title of the news item.</param>
    /// <param name="content">The content of the news item.</param>
    /// <param name="bannerUrl">The URL of the banner image associated with the news item.</param>
    public NewsDto(string title, string content, string bannerUrl)
    {
        Title = title;
        Content = content;
        BannerUrl = bannerUrl;
    }
}
