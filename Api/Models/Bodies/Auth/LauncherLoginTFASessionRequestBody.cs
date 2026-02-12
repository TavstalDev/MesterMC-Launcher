namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class LauncherLoginTFASessionRequestBody
{
    public required string SessionToken { get; init; }
    
    public required string TwoFactorCode { get; init; }
}