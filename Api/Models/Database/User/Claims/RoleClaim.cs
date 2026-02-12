using Microsoft.AspNetCore.Identity;

namespace Tavstal.MesterMC.Api.Models.Database.User.Claims;

public class RoleClaim
{
    public string Key { get; }
    
    public string DefaultValue { get; }
    
    public RoleClaim(string key, string value) 
    { 
        Key = key; 
        DefaultValue = value; 
    }
    
    public IdentityRoleClaim<string> ToIdentityClaim(string roleId)
    {
        return new IdentityRoleClaim<string>
        {
            ClaimType = Key,
            ClaimValue = DefaultValue,
            RoleId = roleId
        };
    }
    
    public IdentityRoleClaim<string> ToIdentityClaim(CustomRole role)
    {
        return ToIdentityClaim(role.Id);
    }
    
    public static List<IdentityRoleClaim<string>> ToList(List<RoleClaim> claims, string roleId)
    {
        List<IdentityRoleClaim<string>> local = new List<IdentityRoleClaim<string>>();
        foreach (var claim in claims)
            local.Add(claim.ToIdentityClaim(roleId));
        return local;
    }
    
    public static List<IdentityRoleClaim<string>> ToList(List<RoleClaim> claims, CustomRole role)
    {
        return ToList(claims, role.Id);
    }
}