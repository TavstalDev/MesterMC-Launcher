namespace Tavstal.MesterMC.Api.Models.Database.User.Claims;

public class CustomClaim
{
    public string Type { get; set; }
    
    public string Value { get; set; }
    
    public CustomClaim(string type, string value)
    {
        Type = type;
        Value = value;
    }
}