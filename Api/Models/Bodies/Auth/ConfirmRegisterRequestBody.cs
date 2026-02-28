namespace Tavstal.MesterMC.Api.Models.Bodies.Auth;

/// <summary>
/// Represents the request body for confirming a user registration.
/// </summary>
public class ConfirmRegisterRequestBody
{
    /// <summary>
    /// Gets or sets the unique identifier of the user.
    /// </summary>
    public required string UserId { get; set; }
    /// <summary>
    /// Gets or sets the confirmation token for verifying the registration.
    /// </summary>
    public required string ConfirmationToken { get; set; } 
}
