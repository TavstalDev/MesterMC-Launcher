namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class ConfirmRegisterRequestBody
{
    public required ulong UserId { get; set; }
    
    public required string ConfirmationToken { get; set; } 
}