namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class LauncherLoginRequestBody
{
    public required string Username { get; init; }
    
    public required string Password { get; init; }
    
    public string? TwoFactorCode { get; init; }
}