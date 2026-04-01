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
        WebsiteUrl = configuration[Constants.ConfigurationKeys.ServerWebsite] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.ServerWebsite); 
        ApiUrl = configuration[Constants.ConfigurationKeys.ServerApi] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.ServerApi);
        
        DatabaseUser = configuration[Constants.ConfigurationKeys.DatabaseUser] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.DatabaseUser); 
        DatabasePassword = configuration[Constants.ConfigurationKeys.DatabasePassword] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.DatabasePassword);
        
        EncryptionKey = configuration[Constants.ConfigurationKeys.JwtEncryptionKey] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.JwtEncryptionKey);
        Issuer = configuration[Constants.ConfigurationKeys.JwtIssuer] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.JwtIssuer);
        Audience = configuration[Constants.ConfigurationKeys.JwtAudience] ?? throw new ArgumentNullException(Constants.ConfigurationKeys.JwtAudience);
        
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

    public Settings(string websiteUrl, string apiUrl, string databaseUser, string databasePassword, string encryptionKey, string issuer, string audience, string emailProvider, int emailPort, string emailAddress, string emailPassword, string[] skinDomains, string certPassword, byte[] cert, string serverName, string implementationName, string implementationVersion)
    {
        WebsiteUrl = websiteUrl;
        ApiUrl = apiUrl;
        DatabaseUser = databaseUser;
        DatabasePassword = databasePassword;
        EncryptionKey = encryptionKey;
        Issuer = issuer;
        Audience = audience;
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