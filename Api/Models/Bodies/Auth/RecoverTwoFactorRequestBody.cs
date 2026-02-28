namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

/// <summary>
/// Represents the request body for recovering a two-factor authentication session.
/// </summary>
public class RecoverTwoFactorRequestBody
{
    /// <summary>
    /// Gets or sets the email address associated with the user's account.
    /// This field is required.
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Gets or sets the backup code used to recover the two-factor authentication session.
    /// This field is required.
    /// </summary>
    public required string BackupCode { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether to log the user out of all active sessions.
    /// This field is optional and defaults to false.
    /// </summary>
    public bool LogoutEverywhere { get; set; } = false;
}
