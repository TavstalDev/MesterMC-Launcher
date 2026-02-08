namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class RecoverPasswordRequestBody
{
    public required string Email { get; set; }
    
    public required string RecoveryToken { get; set; }
    
    public required string NewPassword { get; set; }
    
    public bool LogoutEverywhere { get; set; } = false;
}