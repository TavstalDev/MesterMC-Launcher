namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class RecoverTwoFactorRequestBody
{
    public required string Email { get; set; }
    
    public required string BackupCode { get; set; }
    
    public bool LogoutEverywhere { get; set; } = false;
}