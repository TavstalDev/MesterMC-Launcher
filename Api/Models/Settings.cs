// ReSharper disable NotResolvedInText
namespace Tavstal.MesterMC.Api.Models;

/// <summary>
/// Represents the application settings loaded from the configuration.
/// </summary>
public class Settings
{
    /// <summary>
    /// Gets or sets the website URL.
    /// </summary>
    public string WebsiteUrl { get; set; }

    /// <summary>
    /// Gets or sets the API URL.
    /// </summary>
    public string ApiUrl { get; set; }
    
    /// <summary>
    /// Gets or sets the database username.
    /// </summary>
    public string DatabaseUser { get; set; }

    /// <summary>
    /// Gets or sets the database password.
    /// </summary>
    public string DatabasePassword { get; set; }
    
    /// <summary>
    /// Gets or sets the encryption key used for JWT.
    /// </summary>
    public string EncryptionKey { get; set; }

    /// <summary>
    /// Gets or sets the JWT issuer.
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the JWT audience.
    /// </summary>
    public string Audience { get; set; }
    
    /// <summary>
    /// Gets or sets the clock skew tolerance for JWT token validation.
    /// </summary>
    public TimeSpan ClockSkew { get; set; }
    
    /// <summary>
    /// Maximum number of failed authentication attempts allowed before a user is locked out.
    /// </summary>
    public int LockoutMaxAttempts { get; set; }
    
    /// <summary>
    /// Duration of the lockout applied when the user exceeds the allowed failed authentication attempts.
    /// </summary>
    public TimeSpan LockoutDuration { get; set; }
    
    /// <summary>
    /// Gets or sets the email provider.
    /// </summary>
    public string EmailProvider { get; set; }

    /// <summary>
    /// Gets or sets the email port.
    /// </summary>
    public int EmailPort { get; set; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    public string EmailAddress { get; set; }

    /// <summary>
    /// Gets or sets the email password.
    /// </summary>
    public string EmailPassword { get; set; }
    
    /// <summary>
    /// Gets or sets the allowed skin domains.
    /// </summary>
    public string[] SkinDomains { get; set; }

    /// <summary>
    /// Gets or sets the certificate password.
    /// </summary>
    public string CertPassword { get; set; }

    /// <summary>
    /// Gets or sets the certificate data as a byte array.
    /// </summary>
    public byte[] Cert { get; set; }

    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string ServerName { get; set; }

    /// <summary>
    /// Gets or sets the implementation name.
    /// </summary>
    public string ImplementationName { get; set; }

    /// <summary>
    /// Gets or sets the implementation version.
    /// </summary>
    public string ImplementationVersion { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class and loads settings from the configuration.
    /// </summary>
    /// <param name="configuration">The configuration to load settings from.</param>
    /// <exception cref="ArgumentNullException">Thrown if a required configuration value is missing.</exception>
    public Settings(IConfiguration configuration)
    {
        WebsiteUrl = configuration[Constants.ConfigurationKeys.RuntimeWebsiteUrl] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.RuntimeWebsiteUrl); 
        ApiUrl = configuration[Constants.ConfigurationKeys.RuntimeApiUrl] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.RuntimeApiUrl);
        
        DatabaseUser = configuration[Constants.ConfigurationKeys.DatabaseUser] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.DatabaseUser); 
        DatabasePassword = configuration[Constants.ConfigurationKeys.DatabasePassword] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.DatabasePassword);
        
        EncryptionKey = configuration[Constants.ConfigurationKeys.JwtEncryptionKey] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.JwtEncryptionKey);
        Issuer = configuration[Constants.ConfigurationKeys.JwtIssuer] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.JwtIssuer);
        Audience = configuration[Constants.ConfigurationKeys.JwtAudience] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.JwtAudience);
        ClockSkew = TimeSpan.FromSeconds(configuration.GetValue(Constants.ConfigurationKeys.JwtClockSkew, 5));
        LockoutMaxAttempts = configuration.GetValue(Constants.ConfigurationKeys.JwtLockoutMaxAttempts, 5);
        LockoutDuration = TimeSpan.FromSeconds(configuration.GetValue(Constants.ConfigurationKeys.JwtLockoutDuration, 900)); 
        
        EmailProvider = configuration[Constants.ConfigurationKeys.EmailProvider] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.EmailProvider);
        EmailPort = configuration.GetValue(Constants.ConfigurationKeys.EmailPort, 587); 
        EmailAddress = configuration[Constants.ConfigurationKeys.EmailAddress] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.EmailAddress); 
        EmailPassword = configuration[Constants.ConfigurationKeys.EmailPassword] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.EmailPassword);
        
        SkinDomains = configuration.GetSection(Constants.ConfigurationKeys.YggdrasilSkinDomains).Get<string[]>() ?? throw new ArgumentNullException(Constants.ConfigurationKeys.YggdrasilSkinDomains);
        
        CertPassword = configuration[Constants.ConfigurationKeys.CertPassword] ?? string.Empty;
        if (string.IsNullOrWhiteSpace(CertPassword))
            CertPassword = string.Empty;
        Cert = File.ReadAllBytes(Path.Combine(Program.ContentRoot, "localhost.pfx"));
        ServerName = configuration[Constants.ConfigurationKeys.YggdrasilServerName] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.YggdrasilServerName);
        ImplementationName = configuration[Constants.ConfigurationKeys.YggdrasilImplementationName] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.YggdrasilImplementationName);
        ImplementationVersion = configuration[Constants.ConfigurationKeys.YggdrasilImplementationVersion] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.YggdrasilImplementationVersion);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class with explicit values for all settings.
    /// </summary>
    /// <param name="websiteUrl">The public website URL for the server (e.g., "https://example.com").</param>
    /// <param name="apiUrl">The base API URL that clients will use to access the API (e.g., "https://api.example.com").</param>
    /// <param name="databaseUser">The username used to connect to the database.</param>
    /// <param name="databasePassword">The password used to connect to the database.</param>
    /// <param name="encryptionKey">The symmetric key used for JWT signing/encryption.</param>
    /// <param name="issuer">The JWT issuer value.</param>
    /// <param name="audience">The JWT audience value.</param>
    /// <param name="clockSkew">The clock skew tolerance for JWT token validation.</param>
    /// <param name="lockoutMaxAttempts">The amount of failed authentication attempts allowed before a user is locked out.</param>
    /// <param name="lockoutDuration">The duration of the lockout applied when the user exceeds the allowed failed authentication attempts.</param>
    /// <param name="emailProvider">The SMTP server hostname or provider identifier used for sending email.</param>
    /// <param name="emailPort">The SMTP port used to send email (commonly 25, 465, or 587).</param>
    /// <param name="emailAddress">The email address used as the sender for outgoing messages.</param>
    /// <param name="emailPassword">The password or app-specific secret for the <paramref name="emailAddress"/> account.</param>
    /// <param name="skinDomains">An array of allowed domains for serving skins (Yggdrasil skin domains).</param>
    /// <param name="certPassword">The password protecting the PFX certificate (if any). Can be empty string if none.</param>
    /// <param name="cert">The raw PFX certificate bytes used for TLS/Yggdrasil endpoints.</param>
    /// <param name="serverName">The server name presented by Yggdrasil-compatible endpoints.</param>
    /// <param name="implementationName">The name of the Yggdrasil implementation (metadata shown to clients).</param>
    /// <param name="implementationVersion">The version string of the Yggdrasil implementation.</param>
    public Settings(string websiteUrl, string apiUrl, string databaseUser, string databasePassword, string encryptionKey, string issuer, string audience, TimeSpan clockSkew, int lockoutMaxAttempts, TimeSpan lockoutDuration, string emailProvider, int emailPort, string emailAddress, string emailPassword, string[] skinDomains, string certPassword, byte[] cert, string serverName, string implementationName, string implementationVersion)
    {
        WebsiteUrl = websiteUrl;
        ApiUrl = apiUrl;
        DatabaseUser = databaseUser;
        DatabasePassword = databasePassword;
        EncryptionKey = encryptionKey;
        Issuer = issuer;
        Audience = audience;
        ClockSkew = clockSkew;
        LockoutMaxAttempts = lockoutMaxAttempts;
        LockoutDuration = lockoutDuration;
        EmailProvider = emailProvider;
        EmailPort = emailPort;
        EmailAddress = emailAddress;
        EmailPassword = emailPassword;
        SkinDomains = skinDomains;
        CertPassword = certPassword;
        Cert = cert;
        ServerName = serverName;
        ImplementationName = implementationName;
        ImplementationVersion = implementationVersion;
    }
}