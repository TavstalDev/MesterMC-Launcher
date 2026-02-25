using System.Globalization;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Utils.Helpers;

/// <summary>
/// Provides helper methods for database-related operations, such as generating recovery tokens
/// and retrieving IP information.
/// </summary>
public static class DatabaseHelper
{
    /// <summary>
    /// Generates a unique recovery token that does not already exist in the database.
    /// </summary>
    /// <param name="dbContext">The database context used to check for existing tokens.</param>
    /// <returns>A unique recovery token as a string.</returns>
    public static string GenerateRecoveryToken(CustomDbContext dbContext)
    {
        string key = TokenHelper.GenerateRecoveryToken();
        if (dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.EmailRecoveryToken  && x.ClaimValue == key) != null)
            return GenerateRecoveryToken(dbContext);

        return key;
    }
    
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
            var client = new HttpClient();
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