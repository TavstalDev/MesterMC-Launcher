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
        WebsiteUrl = configuration["Servers:Website"] ?? throw new ArgumentNullException("WebsiteUrl"); 
        ApiUrl = configuration["Servers:API"] ?? throw new ArgumentNullException("ApiUrl");
        
        DatabaseUser = configuration["DB_USER"] ?? throw new ArgumentNullException("DatabaseUser"); 
        DatabasePassword = configuration["DB_PASSWORD"] ?? throw new ArgumentNullException("DatabasePassword");
        
        EncryptionKey = configuration["JWT_ENCRYPTION_KEY"] ?? throw new ArgumentNullException("EncryptionKey");
        Issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
        Audience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
        
        EmailProvider = configuration["Email:Provider"] ?? throw new ArgumentNullException("EmailProvider");
        EmailPort = configuration.GetValue("Email:Port", 587); 
        EmailAddress = configuration["EMAIL_ADDRESS"] ?? throw new ArgumentNullException("EmailAddress"); 
        EmailPassword = configuration["EMAIL_PASSWORD"] ?? throw new ArgumentNullException("EmailPassword");
        
        SkinDomains = configuration.GetSection("Yggdrasil:SkinDomains").Get<string[]>() ?? throw new ArgumentNullException("SkinDomains");
        
        CertPassword = configuration["YGGDRASIL_CERT_PASSWORD"] ?? string.Empty;
        if (CertPassword == " ")
            CertPassword = string.Empty; // Treat a password of " " as an empty password
        Cert = File.ReadAllBytes(Path.Combine(Program.ContentRoot, "localhost.pfx"));
        ServerName = configuration["Yggdrasil:ServerName"] ?? throw new ArgumentNullException("ServerName");
        ImplementationName = configuration["Yggdrasil:ImplementationName"] ?? throw new ArgumentNullException("ImplementationName");
        ImplementationVersion = configuration["Yggdrasil:ImplementationVersion"] ?? throw new ArgumentNullException("ImplementationVersion");
    }
}