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

    #region Login

    /// <summary>
    /// Claim type for tracking the number of login attempts.
    /// </summary>
    public const string LoginAttempt = "mmc.Login.Attempt";

    /// <summary>
    /// Claim type for the expiration time of login attempts.
    /// </summary>
    public const string LoginAttemptExpiration = "mmc.Login.AttemptExpiration";
    #endregion
        
    #region Email
    /// <summary>
    /// Claim type for the email confirmation token.
    /// </summary>
    public const string EmailConfirmationToken = "mmc.Email.ConfirmationToken";
        
    /// <summary>
    /// Claim type for the email recovery expiration.
    /// </summary>
    public const string EmailRecoveryExpiration = "mmc.Email.RecoveryExpiration";
        
    /// <summary>
    /// Claim type for the email recovery token.
    /// </summary>
    public const string EmailRecoveryToken = "mmc.Email.RecoveryToken";
        
    /// <summary>
    /// Claim type for tracking the number of email recovery attempts.
    /// </summary>
    public const string EmailRecoveryAttempt = "mmc.Email.RecoveryAttempt";

    /// <summary>
    /// Claim type for the expiration time of email recovery attempts.
    /// </summary>
    public const string EmailRecoveryAttemptExpiration = "mmc.Email.RecoveryAttemptExpiration";
    #endregion
        
    #region 2FA

    /// <summary>
    /// Claim type for the two-factor authentication recovery code.
    /// </summary>
    public const string TwoFactorRecoveryCode = "mmc.TwoFactor.RecoveryCode";
        
    /// <summary>
    /// Claim type for tracking the number of recovery attempts for two-factor authentication.
    /// </summary>
    public const string TwoFactorRecoveryAttemptCount = "mmc.TwoFactor.RecoveryAttempt";

    /// <summary>
    /// Claim type for the expiration time of recovery attempts for two-factor authentication.
    /// </summary>
    public const string TwoFactorRecoveryAttemptExpiry = "mmc.TwoFactor.RecoveryAttemptExpiration";
        
    /// <summary>
    /// Claim type for the two-factor authentication session token.
    /// </summary>
    public const string TwoFactorSessionToken = "mmc.TwoFactor.SessionToken";
        
    /// <summary>
    /// Claim type for the two-factor authentication session expiration.
    /// </summary>
    public const string TwoFactorSessionExpiration = "mmc.TwoFactor.SessionExpiration";
        
    /// <summary>
    /// Claim type for tracking the number of session attempts for two-factor authentication.
    /// </summary>
    public const string TwoFactorSessionAttempt = "mmc.TwoFactor.SessionAttempt";
    
    /// <summary>
    /// Claim type for the two-factor authentication launcher session token.
    /// </summary>
    public const string TwoFactorLauncherSessionToken = "mmc.TwoFactor.Launcher.SessionToken";

    /// <summary>
    /// Claim type for the expiration time of the two-factor authentication launcher session.
    /// </summary>
    public const string TwoFactorLauncherSessionExpiration = "mmc.TwoFactor.Launcher.SessionExpiration";

    /// <summary>
    /// Claim type for tracking the number of session attempts for the two-factor authentication launcher.
    /// </summary>
    public const string TwoFactorLauncherSessionAttempt = "mmc.TwoFactor.Launcher.SessionAttempt";

    #endregion
}