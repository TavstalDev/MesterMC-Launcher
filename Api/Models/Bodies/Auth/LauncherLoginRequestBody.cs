using System.ComponentModel.DataAnnotations;

namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

/// <summary>
/// Represents the request body for logging into the launcher.
/// </summary>
public class LauncherLoginRequestBody
{
    /// <summary>
    /// Gets or initializes the username of the user.
    /// </summary>
    [Required]
    public required string Username { get; init; }
    
    /// <summary>
    /// Gets or initializes the password of the user.
    /// </summary>
    [Required]
    public required string Password { get; init; }
    
    /// <summary>
    /// Gets or initializes the two-factor authentication code, if applicable.
    /// This property is optional.
    /// </summary>
    public string? TwoFactorCode { get; init; }
}
