namespace Tavstal.MesterMC.Api.Models.Database.User.Claims;

public class UserClaim
{
    public string Key { get; set; }
    
    public string DefaultValue { get; set; }
    
    public UserClaim(string key, string value) { Key = key; DefaultValue = value; }
    
    public CustomUserClaim ToIdentityClaim(string userId)
    {
        return new CustomUserClaim
        {
            ClaimType = Key,
            ClaimValue = DefaultValue,
            UserId = userId
        };
    }
    
    public CustomUserClaim ToIdentityClaim(CustomUser user)
    {
        return ToIdentityClaim(user.Id);
    }
    
    public static List<CustomUserClaim> ToList(List<UserClaim> claims, string userId)
    {
        List<CustomUserClaim> local = new List<CustomUserClaim>();
        foreach (var claim in claims)
            local.Add(claim.ToIdentityClaim(userId));
        return local;
    }
    
    public static List<CustomUserClaim> ToList(List<UserClaim> claims, CustomUser user)
    {
        return ToList(claims, user.Id);
    }
}