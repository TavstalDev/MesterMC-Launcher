namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class LoginRequestBody
{
    public required string Email { get; init; }
    
    public required string Password { get; init; }
    
    public string? TwoFactorCode { get; init; }
    
    public string? TwoFactorRecoveryCode { get; init; }
    
    public bool RememberMe { get; init; }
}