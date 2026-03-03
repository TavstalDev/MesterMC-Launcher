using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models;

public class NewsData
{
    [JsonPropertyName("Title"), JsonProperty("Title")]
    public string Title { get; set; }
    [JsonPropertyName("Content"), JsonProperty("Content")]
    public string Content { get; set; }
    [JsonPropertyName("BannerUrl"), JsonProperty("BannerUrl")]
    public string BannerUrl { get; set; }
    
    public NewsData(string title, string content, string bannerUrl)
    {
        Title = title;
        Content = content;
        BannerUrl = bannerUrl;
    }
}