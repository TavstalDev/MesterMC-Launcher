using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;
using Tavstal.MesterMC.Api.Models.Database.Server;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

/// <summary>
/// Controller for handling Yggdrasil session server-related API requests.
/// </summary>
[ApiController]
[Route("yggdrasil/session/minecraft")] // Client
[Route("yggdrasil/sessionserver/session/minecraft")] // Server
[Tags("Yggdrasil")]
public class SessionServerController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly Settings _settings;
    private readonly ExpiringCache<string, string> _profileCache = new(TimeSpan.FromMinutes(5));
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SessionServerController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="userManager">The user manager for handling user-related operations.</param>
    /// <param name="dbContext">The database context for accessing data.</param>
    /// <param name="settings">The application settings.</param>
    public SessionServerController(ILogger<SessionServerController> logger, CustomUserManager userManager, CustomDbContext dbContext, Settings settings) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _settings = settings;
    }
    /// <summary>
    /// Retrieves the list of blocked servers.
    /// </summary>
    /// <returns>A JSON response containing an empty list of blocked servers.</returns>
    [HttpGet("/yggdrasil/sessionserver/blockedservers")]
    public Task<IActionResult> GetBlockedServers()
    {
        return Task.FromResult(ReturnJson(new { blockedServers = Array.Empty<string>() }));
    }

    /// <summary>
    /// Handles the join request for a server.
    /// </summary>
    /// <param name="request">The join server request containing the selected profile and access token.</param>
    /// <returns>
    /// A response indicating the result of the join operation.
    /// </returns>
    /// <response code="204">The join operation was successful.</response>
    /// <response code="404">User or session not found.</response>
    /// <response code="401">The access token has expired.</response>
    /// <response code="403">The IP address does not match the request.</response>
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] YigJoinServerRequest request)
    {
        string dashedUuid = Guid.Parse(request.selectedProfile).ToString("D");
        CustomUser? user = await _userManager.FindByIdAsync(dashedUuid);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "User not found for the provided selectedProfile UUID");
        
        UserPlaySession? session = await _dbContext.FindUserPlaySessionAsync(x => x.Token == request.accessToken);
        if (session == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No active session found for the provided access token");
        
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (now > session.ExpiresAt) 
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "The access token has expired");
        
        string host = HttpContext.Request.Host.Host;
        if (session.UserIp != host)
            return ReturnResponseCode(HttpStatusCode.Forbidden, "The IP address associated with the access token does not match the IP address of the request");
        
        await _dbContext.AddServerJoinAsync(new ServerJoin
        {
            ServerId = request.serverId,
            UserId = user.Id, 
            UserIp = host, 
            CreatedAt = now, 
            ExpiresAt = now.AddDays(1) 
        }, true);
        return ReturnResponseCode(HttpStatusCode.NoContent);
    }

    /// <summary>
    /// Checks if a user has joined a server.
    /// </summary>
    /// <param name="serverId">The ID of the server.</param>
    /// <param name="username">The username of the user.</param>
    /// <param name="ip">The optional IP address of the user.</param>
    /// <returns>
    /// A JSON response containing the user's profile if the join is valid.
    /// </returns>
    /// <response code="200">The user has joined the server.</response>
    /// <response code="404">User or server join not found.</response>
    /// <response code="401">The server join has expired.</response>
    /// <response code="403">The username does not match the server join.</response>
    [HttpGet("hasJoined")]
    public async Task<IActionResult> HasJoined([FromQuery] string serverId, [FromQuery] string username, [FromQuery] string? ip)
    {
        CustomUser? user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "User not found");
        
        ServerJoin? join = await _dbContext.FindServerJoinAsync(x => x.ServerId == serverId && (x.UserIp == ip || ip == null));
        if (join == null) 
            return ReturnResponseCode(HttpStatusCode.NotFound, "No matching server join found for the provided serverId and IP address");
        
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (now > join.ExpiresAt) 
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "The server join has expired");
        
        if (user.Id != join.UserId)
            return ReturnResponseCode(HttpStatusCode.Forbidden, "The provided username does not match the user associated with the server join");
        
        string json = await GetProfileResponseJsonAsync(user);
        _profileCache.Set(user.Id, json); // TODO: Uncomment it for testing, probably caching will not work if timestamp is a dependency in mojang's authlib
        return ReturnJson(json);
    }

    /// <summary>
    /// Retrieves the profile of a user by UUID.
    /// </summary>
    /// <param name="uuid">The UUID of the user.</param>
    /// <param name="unsigned">Indicates whether the profile should be unsigned.</param>
    /// <returns>
    /// A JSON response containing the user's profile.
    /// </returns>
    /// <response code="200">The profile was retrieved successfully.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">An error occurred while processing the request.</response>
    [HttpGet("profile/{uuid}")]
    public async Task<IActionResult> GetProfile(string uuid, [FromQuery] bool unsigned = true)
    {
        try
        {
            string dashedUuid = Guid.Parse(uuid).ToString("D");
            CustomUser? user = await _userManager.FindByIdAsync(dashedUuid);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found");

            string json = await GetProfileResponseJsonAsync(user, unsigned);
            _profileCache.Set(user.Id,
                json); // TODO: Uncomment it for testing, probably caching will not work if timestamp is a dependency in mojang's authlib
            return ReturnJson(json);
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Unknown error while processing profile request for uuid: {Uuid}, unsigned: {Unsigned}. Error: {ErrorMessage}", uuid, unsigned, ex);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An error occurred while processing the request: " + ex.Message);
        }
    }

    /// <summary>
    /// Generates a JSON response containing the profile information of a user, including textures for skin and cape.
    /// </summary>
    /// <param name="user">The user whose profile information is being retrieved.</param>
    /// <param name="unsigned">Indicates whether the profile should be unsigned. Defaults to true.</param>
    /// <returns>
    /// A JSON string representing the user's profile, including textures and other metadata.
    /// </returns>
    private async Task<string> GetProfileResponseJsonAsync(CustomUser user, bool unsigned = true)
    {
        if (_profileCache.TryGet(user.Id, out var profile))
            return profile;

        Dictionary<string, object> textures = new  Dictionary<string, object>();
        var skin = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
        if (skin != null)
        {
            textures.Add("SKIN", new Dictionary<string, object>
            {
                { "url", skin.GetUrl(_settings.ApiUrl) },
                { "metadata", new Dictionary<string, object> 
                    {
                        {
                            "model", user.GetSkinVariant()
                        }
                    } 
                }
            });
        }

        var userCape = await _dbContext.FindUserCapeAsync(x => x.UserId == user.Id && x.IsSelected);
        if (userCape != null)
        {
            var cape = await _dbContext.FindCapeAsync(x => x.Id == userCape.CapeId);
            if (cape != null)
            {
                var capeData = await _dbContext.FindFileDataAsync(x => x.Id == cape.FileId);
                if (capeData != null)
                {
                    textures.Add("CAPE", new Dictionary<string, object>
                    {
                        { "url", capeData.GetUrl(_settings.ApiUrl) },
                    });
                }
            }
        }
        
        Dictionary<string, object> textureValues = new Dictionary<string, object>
        {
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }, 
            { "profileId", user.Id }, 
            { "profileName", user.UserName }, 
            { "textures", textures }
        };
        if (!unsigned)
            textureValues.Add("signatureRequired", true);

        string jsonString = JsonConvert.SerializeObject(textureValues, Formatting.None);
        string base64Value = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonString));
        
        List<Dictionary<string, object>> properties = [];
        Dictionary<string, object> texturesProperty = new Dictionary<string, object>
        {
            { "name", "textures" },
            { "value", base64Value },
        };
        if (!unsigned)
        {
            using var cert = X509CertificateLoader.LoadPkcs12(_settings.Cert, _settings.CertPassword);
            using var rsa = cert.GetRSAPrivateKey();
        
            if (rsa != null)
            {
                byte[] dataToSign = Encoding.UTF8.GetBytes(base64Value);
                byte[] signatureBytes = rsa.SignData(dataToSign, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
            
                texturesProperty.Add("signature", Convert.ToBase64String(signatureBytes));
            }
        }

        properties.Add(texturesProperty);
        Dictionary<string, object> response = new Dictionary<string, object>
        {
            { "id", user.Id },
            { "name", user.UserName },
            { "properties", properties }
        };
        return JsonConvert.SerializeObject(response, Formatting.None);
    }
}