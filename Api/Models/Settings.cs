namespace Tavstal.MesterMC.Api.Models;

public class Settings
{
    public string EncryptionKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
 
    public string[] SkinDomains { get; set; }
    public byte[] PfxCert { get; set; }
    public string ServerName { get; set; }
    public string ImplementationName { get; set; }
    public string ImplementationVersion { get; set; }
    
    public Settings(IConfiguration configuration)
    {
        EncryptionKey = configuration["Jwt:EncryptionKey"] ?? throw new ArgumentNullException("EncryptionKey");
        Issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
        Audience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
        SkinDomains = configuration.GetSection("Yggdrasil:SkinDomains").Get<string[]>() ?? throw new ArgumentNullException("SkinDomains");
        PfxCert = File.ReadAllBytes(Path.Combine(Program.ContentRoot, "Certs/yggdrasil.pfx"));
        ServerName = configuration["Yggdrasil:ServerName"] ?? throw new ArgumentNullException("ServerName");
        ImplementationName = configuration["Yggdrasil:ImplementationName"] ?? throw new ArgumentNullException("ImplementationName");
        ImplementationVersion = configuration["Yggdrasil:ImplementationVersion"] ?? throw new ArgumentNullException("ImplementationVersion");
    }
}