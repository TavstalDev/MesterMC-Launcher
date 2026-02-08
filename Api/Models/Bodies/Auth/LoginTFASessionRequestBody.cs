namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class LoginTFASessionRequestBody
{
    public required string TwoFactorCode { get; init; }
    
    public bool RememberMe { get; set; } = false;
}