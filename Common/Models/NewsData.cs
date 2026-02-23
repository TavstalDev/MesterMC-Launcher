using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models;

public class NewsData
{
    [JsonPropertyName("title"), JsonProperty("title")]
    public string Title { get; set; }
    [JsonPropertyName("content"), JsonProperty("content")]
    public string Content { get; set; }
    [JsonPropertyName("banner"), JsonProperty("banner")]
    public string BannerUrl { get; set; }
    
    public NewsData(string title, string content, string bannerUrl)
    {
        Title = title;
        Content = content;
        BannerUrl = bannerUrl;
    }
}