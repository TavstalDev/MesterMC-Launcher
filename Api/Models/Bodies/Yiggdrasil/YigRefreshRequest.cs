namespace Tavstal.MesterMC.Api.Models.Bodies.Yiggdrasil;

public class YigRefreshRequest
{
    public required string accessToken { get; set; }
    public string? clientToken { get; set; }
    public bool requestUser { get; set; }
    public YigProfileBody selectedProfile { get; set; }
}