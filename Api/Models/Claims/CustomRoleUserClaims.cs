using Tavstal.MesterMC.Api.Models.Database.User.Claims;

namespace Tavstal.MesterMC.Api.Models.Claims;

public static class CustomRoleUserClaims
{
    // ReSharper disable once InconsistentNaming
    private static readonly Dictionary<string, List<UserClaim>> _claims = new()
    {
        { "Default", new List<UserClaim>() },
        { "Restricted", new List<UserClaim>() },
        { "UnderTermination", new List<UserClaim>() },
        { "Moderator", new List<UserClaim>
            {
                new(CustomClaimTypes.Badge, "Moderator"),
            }
        },
        { "Admin", new List<UserClaim>
            {
                new(CustomClaimTypes.Badge, "Administrator")
            }
        },
    };

    public static Dictionary<string, List<UserClaim>> Claims => _claims;
}