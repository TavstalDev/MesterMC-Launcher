using System.ComponentModel.DataAnnotations;

namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

/// <summary>
/// Represents the request body for logging into the system.
/// </summary>
public class LoginRequestBody
{
    /// <summary>
    /// Gets or initializes the email address of the user.
    /// This field is required.
    /// </summary>
    [Required]
    public required string Email { get; init; }
    
    /// <summary>
    /// Gets or initializes the password of the user.
    /// This field is required.
    /// </summary>
    [Required]
    public required string Password { get; init; }
    
    /// <summary>
    /// Gets or initializes a value indicating whether the user should remain logged in.
    /// This field is optional and defaults to false.
    /// </summary>
    public bool RememberMe { get; init; }
}
