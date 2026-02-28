using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Common;

namespace Tavstal.MesterMC.Api.Models.Bodies.Account;

/// <summary>
/// Represents the request body for updating a user's profile.
/// </summary>
public class UpdateProfileRequestBody
{
    /// <summary>
    /// Gets or sets the display name of the user.
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Gets or sets the biography of the user.
    /// </summary>
    public string? Biography { get; set; }
    
    /// <summary>
    /// Gets or sets the email address of the user.
    /// </summary>
    public string? Email { get; set; }
    
    /// <summary>
    /// Gets or sets the avatar image file for the user.
    /// The file must be a PNG image and its size should not exceed 1 MB.
    /// </summary>
    [FormFile(1, EFileSizeUnit.Megabytes, ["image/png"], [".png"])]
    public IFormFile? Avatar { get; set; }
}