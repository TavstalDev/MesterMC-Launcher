#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class LoginResponse
{
    public string Message { get; set; } 
    
    public ulong UserId { get; set; } 
    
    public string Token { get; set; } 
    
    public string Expires { get; set; } 
}