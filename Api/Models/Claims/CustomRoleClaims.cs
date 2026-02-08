using Tavstal.MesterMC.Api.Models.Database.User.Claims;

namespace Tavstal.MesterMC.Api.Models.Claims;

public static class CustomRoleClaims
{
    // ReSharper disable once InconsistentNaming
    // TODO: Add claims to the roles
    private static readonly Dictionary<string, List<RoleClaim>> _claims = new()
    {
        { "Default", new List<RoleClaim>() },
        { "Restricted", new List<RoleClaim>() },
        { "UnderTermination", new List<RoleClaim>() },
        { "Moderator", new List<RoleClaim>() },
        { "Admin", new List<RoleClaim>() },
    };

    public static Dictionary<string, List<RoleClaim>> Claims => _claims;
}