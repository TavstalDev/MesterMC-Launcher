using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

namespace Tavstal.KonkordLauncher.Common.Models;

public class NewsData
{
    [JsonPropertyName("title"), JsonProperty("title")]
    public string Title { get; set; }
    [JsonPropertyName("content"), JsonProperty("content")]
    public string Content { get; set; }
    [JsonPropertyName("banner"), JsonProperty("banner")]
    public string BannerPath { get; set; }
    
    public NewsData(string title, string content, string bannerPath)
    {
        Title = title;
        Content = content;
        BannerPath = bannerPath;
    }
    
    public Uri  GetBannerUri()
    {
        return new Uri(new (MesterMcEndpoints.ApiBaseEndpoint), BannerPath);
    }
}