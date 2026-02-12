using System.Diagnostics;

namespace Tavstal.MesterMC.Api.Models;

public class Settings
{
    public string WebsiteUrl { get; set; }
    public string ApiUrl { get; set; }
    
    public string DatabaseUser { get; set; }
    public string DatabasePassword { get; set; }
    
    public string EncryptionKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    
    public string EmailProvider { get; set; }
    public int EmailPort { get; set; }
    public string EmailAddress { get; set; }
    public string EmailPassword { get; set; }
 
    public string[] SkinDomains { get; set; }
    public byte[] Cert { get; set; }
    public string ServerName { get; set; }
    public string ImplementationName { get; set; }
    public string ImplementationVersion { get; set; }
    
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
        
        Cert = File.ReadAllBytes(Path.Combine(Program.ContentRoot, "Certs/yggdrasil.pfx"));
        ServerName = configuration["Yggdrasil:ServerName"] ?? throw new ArgumentNullException("ServerName");
        ImplementationName = configuration["Yggdrasil:ImplementationName"] ?? throw new ArgumentNullException("ImplementationName");
        ImplementationVersion = configuration["Yggdrasil:ImplementationVersion"] ?? throw new ArgumentNullException("ImplementationVersion");
    }
}