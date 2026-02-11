namespace Tavstal.MesterMC.Api.Models.Bodies.Yiggdrasil;

public class YigValidateRequest
{
    public required string accessToken { get; set; }
    public string? clientToken { get; set; }
}