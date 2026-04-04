using System.ComponentModel.DataAnnotations;

namespace Tavstal.MesterMC.Api.Models;

/// <summary>
/// Represents the application settings loaded from the configuration.
/// </summary>
public class Settings
{
    /// <summary>
    /// Gets or sets the website URL.
    /// </summary>
    public string WebsiteUrl { get; }

    /// <summary>
    /// Gets or sets the API URL.
    /// </summary>
    public string ApiUrl { get; }
    
    /// <summary>
    /// Gets or sets the encryption key used for JWT.
    /// </summary>
    public string EncryptionKey { get; }

    /// <summary>
    /// Gets or sets the JWT issuer.
    /// </summary>
    public string Issuer { get; }

    /// <summary>
    /// Gets or sets the JWT audience.
    /// </summary>
    public string Audience { get; }
    
    /// <summary>
    /// Gets or sets the clock skew tolerance for JWT token validation.
    /// </summary>
    public TimeSpan ClockSkew { get; }
    
    /// <summary>
    /// Maximum number of failed authentication attempts allowed before a user is locked out.
    /// </summary>
    public int LockoutMaxAttempts { get; }
    
    /// <summary>
    /// Duration of the lockout applied when the user exceeds the allowed failed authentication attempts.
    /// </summary>
    public TimeSpan LockoutDuration { get; }
    
    /// <summary>
    /// Gets or sets the email provider.
    /// </summary>
    public string EmailProvider { get; }

    /// <summary>
    /// Gets or sets the email port.
    /// </summary>
    public int EmailPort { get; }

    /// <summary>
    /// Gets or sets the email address.
    /// </summary>
    [EmailAddress]
    public string EmailAddress { get; }

    /// <summary>
    /// Gets or sets the email password.
    /// </summary>
    public string EmailPassword { get; }
    
    /// <summary>
    /// Gets or sets the allowed skin domains.
    /// </summary>
    public IReadOnlyList<string> SkinDomains { get; }

    /// <summary>
    /// Gets or sets the certificate fingerprint.
    /// </summary>
    public string CertificateFingerprint { get; }
    
    /// <summary>
    /// Gets or sets the password or passphrase for the SSL/TLS certificate.
    /// This value is required when loading certificates from encrypted PFX/PKCS#12 files.
    /// </summary>
    public string CertificatePassword { get; }
    
    /// <summary>
    /// Gets or sets the server name.
    /// </summary>
    public string ServerName { get; }

    /// <summary>
    /// Gets or sets the implementation name.
    /// </summary>
    public string ImplementationName { get; }

    /// <summary>
    /// Gets or sets the implementation version.
    /// </summary>
    public string ImplementationVersion { get; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class and loads settings from the configuration.
    /// </summary>
    /// <param name="configuration">The configuration to load settings from.</param>
    /// <exception cref="ArgumentNullException">Thrown if a required configuration value is missing.</exception>
    public Settings(IConfiguration configuration)
    {
        WebsiteUrl = GetString(configuration, Constants.ConfigurationKeys.RuntimeWebsiteUrl);
        ApiUrl = GetString(configuration, Constants.ConfigurationKeys.RuntimeApiUrl);

        EncryptionKey = GetString(configuration, Constants.EnvironmentKeys.JwtEncryptionKey);
        if (string.IsNullOrWhiteSpace(EncryptionKey) || EncryptionKey.Length < 32)
            throw new ArgumentException("Encryption key must be at least 32 characters.", nameof(EncryptionKey));

        Issuer = GetString(configuration, Constants.ConfigurationKeys.JwtIssuer);
        Audience = GetString(configuration, Constants.ConfigurationKeys.JwtAudience);
        ClockSkew = TimeSpan.FromSeconds(configuration.GetValue(Constants.ConfigurationKeys.JwtClockSkew, 5));
        LockoutMaxAttempts = configuration.GetValue(Constants.ConfigurationKeys.JwtLockoutMaxAttempts, 5);
        LockoutDuration = TimeSpan.FromSeconds(configuration.GetValue(Constants.ConfigurationKeys.JwtLockoutDuration, 900));

        EmailProvider = GetString(configuration, Constants.ConfigurationKeys.EmailProvider);
        EmailPort = configuration.GetValue(Constants.ConfigurationKeys.EmailPort, 587); 
        EmailAddress = GetString(configuration, Constants.EnvironmentKeys.EmailAddress);
        EmailPassword = GetString(configuration, Constants.EnvironmentKeys.EmailPassword);
        
        SkinDomains = configuration.GetSection(Constants.ConfigurationKeys.YggdrasilSkinDomains).Get<string[]>() ?? throw new ArgumentNullException(Constants.ConfigurationKeys.YggdrasilSkinDomains);
        
        CertificateFingerprint = GetString(configuration, Constants.EnvironmentKeys.CertificateFingerprint);
        CertificatePassword = GetString(configuration, Constants.EnvironmentKeys.CertificatePassword);
        
        ServerName = GetString(configuration, Constants.ConfigurationKeys.YggdrasilServerName);
        ImplementationName = GetString(configuration, Constants.ConfigurationKeys.YggdrasilImplementationName);
        ImplementationVersion = GetString(configuration, Constants.ConfigurationKeys.YggdrasilImplementationVersion);
    }

    #if DEBUG
    /// <summary>
    /// Initializes a new instance of the <see cref="Settings"/> class with explicit values for all settings.
    /// </summary>
    /// <param name="websiteUrl">The public website URL for the server (e.g., "https://example.com").</param>
    /// <param name="apiUrl">The base API URL that clients will use to access the API (e.g., "https://api.example.com").</param>
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
    /// <param name="certificateFingerprint">The fingerprint of the TLS certificate used for secure communication (optional, can be empty).</param>
    /// <param name="certificatePassword">The password or passphrase for the TLS certificate (required if using an encrypted PFX/PKCS#12 file).</param>
    /// <param name="serverName">The server name presented by Yggdrasil-compatible endpoints.</param>
    /// <param name="implementationName">The name of the Yggdrasil implementation (metadata shown to clients).</param>
    /// <param name="implementationVersion">The version string of the Yggdrasil implementation.</param>
    public Settings(string websiteUrl, string apiUrl, string encryptionKey, string issuer, 
        string audience, TimeSpan clockSkew, int lockoutMaxAttempts, TimeSpan lockoutDuration, string emailProvider, int emailPort, 
        string emailAddress, string emailPassword, string[] skinDomains, string certificateFingerprint, string certificatePassword,
        string serverName, string implementationName, string implementationVersion)
    {
        WebsiteUrl = websiteUrl;
        ApiUrl = apiUrl;
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
        CertificateFingerprint = certificateFingerprint;
        CertificatePassword = certificatePassword;
        ServerName = serverName;
        ImplementationName = implementationName;
        ImplementationVersion = implementationVersion;
    }
    #endif

    private string GetString(IConfiguration configuration, string key) => configuration[key] ?? throw new ArgumentNullException(key);
}