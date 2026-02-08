using Tavstal.MesterMC.Api.Models.Attributes;

namespace Tavstal.MesterMC.Api.Models.Bodies.Account;

public class UpdateProfileRequestBody
{
    public string? DisplayName { get; set; }
    
    public string? Biography { get; set; }
    
    public string? Email { get; set; }
    
    [FormFile(1, EFileSizeUnit.Megabytes, ["image/png"], [".png"])]
    public IFormFile? Avatar { get; set; }
}