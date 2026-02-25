using Newtonsoft.Json;

namespace Tavstal.MesterMC.Api.Models.Common;

public class IpInfo
{
    [JsonProperty("ip")]
    public string Ip { get; set; }
    
    [JsonProperty("hostname")]
    public string Hostname { get; set; }
    
    [JsonProperty("city")]
    public string City { get; set; }
    
    [JsonProperty("region")]
    public string Region { get; set; }
    
    [JsonProperty("country")]
    public string Country { get; set; }
    
    [JsonProperty("loc")]
    public string Loc { get; set; }
    
    [JsonProperty("org")]
    public string Organization { get; set; }
    
    [JsonProperty("postal")]
    public string Postal { get; set; }
        

    public IpInfo(string ip, string hostname, string city, string region, string country, string loc, string organization, string postal)
    {
        Ip = ip;
        Hostname = hostname;
        City = city;
        Region = region;
        Country = country;
        Loc = loc;
        Organization = organization;
        Postal = postal;
    }
}