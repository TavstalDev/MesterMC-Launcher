using System.Globalization;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models.Common;

namespace Tavstal.MesterMC.Api.Utils.Helpers;

/// <summary>
/// Provides helper methods for database-related operations
/// </summary>
public static class DatabaseHelper
{
    /// <summary>
    /// Retrieves information about an IP address, such as country, region, and city.
    /// If the information cannot be retrieved, returns default values.
    /// </summary>
    /// <param name="ip">The IP address to retrieve information for.</param>
    /// <returns>An <see cref="IpInfo"/> object containing the IP information.</returns>
    public static async Task<IpInfo> GetIpInformation(string ip)
    {
        IpInfo? ipInfo = null;
        try
        {
            using var client = new HttpClient();
            string info = await client.GetStringAsync("https://ipinfo.io/" + ip);
            ipInfo = JsonConvert.DeserializeObject<IpInfo>(info);
            if (ipInfo != null)
                ipInfo.Country = new RegionInfo(ipInfo.Country).EnglishName;
        }
        catch
        {
            // Ignore
        }

        ipInfo ??= new IpInfo(
            ip,
            "Unknown",
            "Unknown",
            "Unknown",
            "Unknown",
            "0.0 0.0",
            "Unknown",
            "Unknown"
        );
        return ipInfo;
    }
}