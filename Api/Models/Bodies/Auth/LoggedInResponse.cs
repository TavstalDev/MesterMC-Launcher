using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Models.Database.User.Claims;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class LoggedInResponse
{
    public ulong UserId { get; set; }
    
    public string Username { get; set; }
    
    public string DisplayName { get; set; }
    
    public string Email { get; set; }
    
    public bool HasAvatar { get; set; }
    
    public string Avatar { get; set; }
    
    public List<CustomRole> Roles { get; set; }
    
    public List<CustomClaim> Claims { get; set; }
}