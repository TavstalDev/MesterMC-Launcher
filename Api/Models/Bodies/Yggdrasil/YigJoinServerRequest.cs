namespace Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;

public class YigJoinServerRequest
{
    public required string accessToken { get; set; }
    // uuid of the profile
    public required string selectedProfile { get; set; }
    public required string serverId { get; set; }
}