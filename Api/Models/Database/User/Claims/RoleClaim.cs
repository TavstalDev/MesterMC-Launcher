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
    
    public IdentityRoleClaim<ulong> ToIdentityClaim(ulong roleId)
    {
        return new IdentityRoleClaim<ulong>
        {
            ClaimType = Key,
            ClaimValue = DefaultValue,
            RoleId = roleId
        };
    }
    
    public IdentityRoleClaim<ulong> ToIdentityClaim(CustomRole role)
    {
        return ToIdentityClaim(role.Id);
    }
    
    public static List<IdentityRoleClaim<ulong>> ToList(List<RoleClaim> claims, ulong roleId)
    {
        List<IdentityRoleClaim<ulong>> local = new List<IdentityRoleClaim<ulong>>();
        foreach (var claim in claims)
            local.Add(claim.ToIdentityClaim(roleId));
        return local;
    }
    
    public static List<IdentityRoleClaim<ulong>> ToList(List<RoleClaim> claims, CustomRole role)
    {
        return ToList(claims, role.Id);
    }
}