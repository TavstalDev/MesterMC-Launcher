namespace Tavstal.MesterMC.Launcher.Models;

/// <summary>
/// Represents the various login statuses that can occur during the authentication process.
/// </summary>
public enum ELoginStatus
{
    /// <summary>
    /// No login status is set.
    /// </summary>
    NONE = 0,

    /// <summary>
    /// The user is currently logging in.
    /// </summary>
    LOGGING_IN = 1,

    /// <summary>
    /// The login process was successful.
    /// </summary>
    SUCCESS = 2,

    /// <summary>
    /// The application is launching after a successful login.
    /// </summary>
    LAUNCHING = 3,

    /// <summary>
    /// An error occurred during the login process.
    /// </summary>
    ERROR = 4,

    /// <summary>
    /// Two-factor authentication is required to complete the login process.
    /// </summary>
    TFA = 5
}
