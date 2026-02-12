namespace Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;

public class YigLoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public string? ClientToken { get; set; }
    public bool RequestUser { get; set; }
}