namespace Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;

/// <summary>
/// Represents the request body for joining a server in the Yggdrasil API.
/// </summary>
public class YigJoinServerRequest
{
    /// <summary>
    /// Gets or sets the access token used for authentication.
    /// This field is required.
    /// </summary>
    public required string accessToken { get; set; }

    /// <summary>
    /// Gets or sets the UUID of the selected profile.
    /// This field is required.
    /// </summary>
    public required string selectedProfile { get; set; }

    /// <summary>
    /// Gets or sets the server ID to join.
    /// This field is required.
    /// </summary>
    public required string serverId { get; set; }
}
