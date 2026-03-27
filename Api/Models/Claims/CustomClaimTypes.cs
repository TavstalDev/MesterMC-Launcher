namespace Tavstal.MesterMC.Api.Models.Claims;

/// <summary>
/// Provides custom claim types for the application.
/// </summary>
public static class CustomClaimTypes
{
    /// <summary>
    /// Claim type for user badges.
    /// </summary>
    public const string Badge = "mmc.User.Badge";
    
    /// <summary>
    /// Claim type for the email confirmation token.
    /// </summary>
    public const string EmailConfirmationToken = "mmc.Email.ConfirmationToken";
}