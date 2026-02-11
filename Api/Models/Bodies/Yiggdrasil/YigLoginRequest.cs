namespace Tavstal.MesterMC.Api.Models.Bodies.Yiggdrasil;

public class YigLoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? ClientToken { get; set; }
    public bool RequestUser { get; set; }
}