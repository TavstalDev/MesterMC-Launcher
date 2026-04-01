namespace Tavstal.MesterMC.Api;

public static class Constants
{
    public static class Authentication
    {
        /// <summary>
        /// Defines the allowed characters in usernames for identity validation.
        /// </summary>
        public const string AllowedUsernameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
    }

    public static class ConfigurationKeys
    {
        public const string ServerWebsite = "Servers:Website";
        public const string ServerApi = "Servers:API";
        public const string DatabaseUser = "DB_USER";
        public const string DatabasePassword = "DB_PASSWORD";
        public const string JwtEncryptionKey = "JWT_ENCRYPTION_KEY";
        public const string JwtIssuer = "Jwt:Issuer";
        public const string JwtAudience = "Jwt:Audience";
        public const string EmailProvider = "Email:Provider";
        public const string EmailPort = "Email:Port";
        public const string EmailAddress = "EMAIL_ADDRESS";
        public const string EmailPassword = "EMAIL_PASSWORD";
        public const string YggdrasilSkinDomains = "Yggdrasil:SkinDomains";
        public const string CertPassword = "CERT_PASSWORD";
        public const string YggdrasilServerName = "Yggdrasil:ServerName";
        public const string YggdrasilImplementationName = "Yggdrasil:ImplementationName";
        public const string YggdrasilImplementationVersion = "Yggdrasil:ImplementationVersion";
    }
}