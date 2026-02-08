namespace Tavstal.MesterMC.Api.Models;

public class JwtSettings
{
    public string EncryptionKey { get; set; }
    
    public string Issuer { get; set; }
    
    public string Audience { get; set; }
    
    public JwtSettings(IConfiguration configuration)
    {
        EncryptionKey = configuration["EncryptionKey"] ?? throw new ArgumentNullException("EncryptionKey");
        Issuer = configuration["Jwt:Issuer"] ?? throw new ArgumentNullException("Jwt:Issuer");
        Audience = configuration["Jwt:Audience"] ?? throw new ArgumentNullException("Jwt:Audience");
    }
}