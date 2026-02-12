namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class ConfirmRegisterRequestBody
{
    public required string UserId { get; set; }
    
    public required string ConfirmationToken { get; set; } 
}