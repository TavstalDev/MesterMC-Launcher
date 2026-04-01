namespace Tavstal.MesterMC.Api;

/// <summary>
/// Holds project-wide constant values used for configuration keys and authentication rules.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Authentication-related constants and validation rules.
    /// </summary>
    public static class Authentication
    {
        /// <summary>
        /// Defines the allowed characters in usernames for identity validation.
        /// Only characters present in this string are permitted when creating or validating usernames.
        /// The underscore character is included to allow simple separators in usernames.
        /// </summary>
        public const string AllowedUsernameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    }

    /// <summary>
    /// Configuration key names used to read settings from configuration providers
    /// (appsettings.json, environment variables, .env loader, etc.).
    /// Use these constants when accessing IConfiguration to avoid typo-related bugs.
    /// </summary>
    public static class ConfigurationKeys
    {
        /// <summary>
        /// The configuration key for the public-facing website URL of hosted game servers.
        /// Example: "https://example.com"
        /// </summary>
        public const string ServerWebsite = "Servers:Website";

        /// <summary>
        /// The configuration key for the API base URL.
        /// Example: "https://api.example.com"
        /// </summary>
        public const string ServerApi = "Servers:API";

        /// <summary>
        /// Environment/config key for the database username (used to build the DB connection string).
        /// </summary>
        public const string DatabaseUser = "DB_USER";

        /// <summary>
        /// Environment/config key for the database password (used to build the DB connection string).
        /// </summary>
        public const string DatabasePassword = "DB_PASSWORD";

        /// <summary>
        /// Environment/config key for the symmetric key used to sign/encrypt JWTs.
        /// This value must be kept secret.
        /// </summary>
        public const string JwtEncryptionKey = "JWT_ENCRYPTION_KEY";

        /// <summary>
        /// Configuration key for the JWT token issuer value.
        /// </summary>
        public const string JwtIssuer = "Jwt:Issuer";

        /// <summary>
        /// Configuration key for the JWT token audience value.
        /// </summary>
        public const string JwtAudience = "Jwt:Audience";

        /// <summary>
        /// Configuration key for the email (SMTP) provider hostname.
        /// Example: "smtp.example.com"
        /// </summary>
        public const string EmailProvider = "Email:Provider";

        /// <summary>
        /// Configuration key for the email (SMTP) provider port.
        /// Example: 25, 465, or 587
        /// </summary>
        public const string EmailPort = "Email:Port";

        /// <summary>
        /// Environment/config key for the email sender address used by the app.
        /// Example: "noreply@example.com"
        /// </summary>
        public const string EmailAddress = "EMAIL_ADDRESS";

        /// <summary>
        /// Environment/config key for the email sender account password / app secret.
        /// </summary>
        public const string EmailPassword = "EMAIL_PASSWORD";

        /// <summary>
        /// Configuration key containing an array/list of allowed skin domains for Yggdrasil
        /// (used when validating or serving player skins).
        /// </summary>
        public const string YggdrasilSkinDomains = "Yggdrasil:SkinDomains";

        /// <summary>
        /// Environment/config key for the certificate (PFX) password, if a PFX is used.
        /// </summary>
        public const string CertPassword = "CERT_PASSWORD";

        /// <summary>
        /// Configuration key for the Yggdrasil-compatible server display name.
        /// </summary>
        public const string YggdrasilServerName = "Yggdrasil:ServerName";

        /// <summary>
        /// Configuration key for the Yggdrasil implementation name (metadata returned to clients).
        /// </summary>
        public const string YggdrasilImplementationName = "Yggdrasil:ImplementationName";

        /// <summary>
        /// Configuration key for the Yggdrasil implementation version string (metadata returned to clients).
        /// </summary>
        public const string YggdrasilImplementationVersion = "Yggdrasil:ImplementationVersion";
    }
}