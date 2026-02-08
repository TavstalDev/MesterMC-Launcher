using System.Globalization;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Utils.Helpers;

public static class DatabaseHelper
{
    public static string GenerateRecoveryToken(CustomDbContext dbContext)
    {
        string key = TokenHelper.GenerateRecoveryToken();
        if (dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.EmailRecoveryToken  && x.ClaimValue == key) != null)
            return GenerateRecoveryToken(dbContext);

        return key;
    }
    
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

        if (ipInfo == null)
            ipInfo = new IpInfo(

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