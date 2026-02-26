namespace Tavstal.MesterMC.Api.Models;

/// <summary>
/// A static class that defines constant strings representing various rate limit categories.
/// These categories can be used to identify and enforce rate limiting policies in the application.
/// </summary>
public static class RateLimits
{
    /// <summary>
    /// Represents the default rate limit category.
    /// </summary>
    public const string DEFAULT = "default";

    /// <summary>
    /// Represents the rate limit category for user registration actions.
    /// </summary>
    public const string AUTH_REGISTER = "auth_register";
    /// <summary>
    /// Represents the rate limit category for user login actions.
    /// </summary>
    public const string AUTH_LOGIN = "auth_login";

    /// <summary>
    /// Represents the rate limit category for password reset actions.
    /// </summary>
    public const string AUTH_RESET = "auth_reset";

    /// <summary>
    /// Represents the rate limit category for file upload actions.
    /// </summary>
    public const string UPLOAD = "upload";

    /// <summary>
    /// Represents the rate limit category for file download actions.
    /// </summary>
    public const string DOWNLOAD = "download";

    /// <summary>
    /// Represents the rate limit category for search actions.
    /// </summary>
    public const string SEARCH = "search";

    /// <summary>
    /// Represents the rate limit category for write actions.
    /// </summary>
    public const string WRITE = "write";

    /// <summary>
    /// Represents the rate limit category for administrative actions.
    /// </summary>
    public const string ADMIN = "admin";
}
