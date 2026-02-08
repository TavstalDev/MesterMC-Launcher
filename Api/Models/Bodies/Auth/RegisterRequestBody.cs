using System.ComponentModel.DataAnnotations;
using Tavstal.MesterMC.Api.Models.Attributes;

namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

public class RegisterRequestBody
{
    [Required]
    [MinLength(3)]
    [MaxLength(16)]
    public string Username { get; set; }
    
    [Required]
    [MinLength(5)]
    [MaxLength(320)]
    public string EmailAddress { get; set; }
    
    [Required]
    [MinLength(8)]
    [MaxLength(64)]
    public string Password { get; set; }
    
    [FormFile(1, EFileSizeUnit.Megabytes, ["image/png"], [".png"])]
    public IFormFile? Avatar { get; set; }
}