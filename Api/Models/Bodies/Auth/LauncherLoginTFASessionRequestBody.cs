namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

/// <summary>
/// Represents the request body for logging into the launcher with a two-factor authentication session.
/// </summary>
public class LauncherLoginTFASessionRequestBody
{
    /// <summary>
    /// Gets or initializes the session token for the login session.
    /// This token is required to authenticate the session.
    /// </summary>
    public required string SessionToken { get; init; }
    
    /// <summary>
    /// Gets or initializes the two-factor authentication code.
    /// This code is required to complete the login process.
    /// </summary>
    public required string TwoFactorCode { get; init; }
}
