using System.ComponentModel.DataAnnotations;

namespace Tavstal.MesterMC.Api.Models.Bodies.Account;

public class UpdatedPasswordRequestBody
{
    [Required]
    public required string CurrentPassword { get; set; }
    
    [Required]
    public required string NewPassword { get; set; }
    
    public bool LogoutEverywhere { get; set; } = false;
}