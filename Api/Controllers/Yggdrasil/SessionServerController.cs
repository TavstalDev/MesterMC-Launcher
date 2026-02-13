using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;
using Tavstal.MesterMC.Api.Models.Database.Server;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

[ApiController]
[Route("yggdrasil/session/minecraft")] // Client
[Route("yggdrasil/sessionserver/session/minecraft")] // Server
[Tags("Yggdrasil")]
public class SessionServerController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly Settings _settings;
    private readonly ExpiringCache<string, string> _profileCache = new(TimeSpan.FromMinutes(5));
    
    public SessionServerController(IConfiguration configuration, ILogger<SessionServerController> logger, CustomUserManager userManager, CustomDbContext dbContext, Settings settings)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
        _settings = settings;
    }
    
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] YigJoinServerRequest request)
    {
        _logger.LogWarning("Join request for serverId: {ServerId}, selectedProfile: {SelectedProfile}, accessToken: {AccessToken}", request.serverId, request.selectedProfile, request.accessToken);
        string dashedUuid = Guid.Parse(request.selectedProfile).ToString("D");
        CustomUser? user = await _userManager.FindByIdAsync(dashedUuid);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.NotFound, "User not found for the provided selectedProfile UUID");
        
        UserPlaySession? session = _dbContext.FindUserPlaySession(x => x.Token == request.accessToken);
        if (session == null)
            return this.ReturnResponseCode(HttpStatusCode.NotFound, "No active session found for the provided access token");
        
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (now > session.ExpiresAt) 
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "The access token has expired");
        
        string host = HttpContext.Request.Host.Host;
        if (session.UserIp != host)
            return this.ReturnResponseCode(HttpStatusCode.Forbidden, "The IP address associated with the access token does not match the IP address of the request");
        
        await _dbContext.AddServerJoinAsync(new ServerJoin
        {
            ServerId = request.serverId,
            UserId = user.Id, 
            UserIp = host, 
            CreatedAt = now, 
            ExpiresAt = now.AddDays(1) 
        }, true);
        return this.ReturnResponseCode(HttpStatusCode.NoContent);
    }

    [HttpGet("hasJoined")]
    public async Task<IActionResult> HasJoined([FromQuery] string serverId, [FromQuery] string username, [FromQuery] string? ip)
    {
        _logger.LogWarning("HasJoined request for serverId: {ServerId}, username: {Username}, ip: {Ip}", serverId, username, ip);
        CustomUser? user = await _userManager.FindByNameAsync(username);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.NotFound, "User not found");
        
        ServerJoin? join = _dbContext.FindServerJoin(x => x.ServerId == serverId && (x.UserIp == ip || ip == null));
        if (join == null) 
            return this.ReturnResponseCode(HttpStatusCode.NotFound, "No matching server join found for the provided serverId and IP address");
        
        DateTimeOffset now = DateTimeOffset.UtcNow;
        if (now > join.ExpiresAt) 
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "The server join has expired");
        
        if (user.Id != join.UserId)
            return this.ReturnResponseCode(HttpStatusCode.Forbidden, "The provided username does not match the user associated with the server join");
        
        string json = await GetProfileResponseJsonAsync(user);
        _profileCache.Set(user.Id, json); // TODO: Uncomment it for testing, probably caching will not work if timestamp is a dependency in mojang's authlib
        return this.ReturnJson(json);
    }

    [HttpGet("profile/{uuid}")]
    public async Task<IActionResult> GetProfile(string uuid, [FromQuery] bool unsigned = true)
    {
        try
        {
            string dashedUuid = Guid.Parse(uuid).ToString("D");
            CustomUser? user = await _userManager.FindByIdAsync(dashedUuid);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "User not found");

            string json = await GetProfileResponseJsonAsync(user, unsigned);
            _profileCache.Set(user.Id,
                json); // TODO: Uncomment it for testing, probably caching will not work if timestamp is a dependency in mojang's authlib
            return this.ReturnJson(json);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Unknown error while processing profile request for uuid: {Uuid}, unsigned: {Unsigned}. Error: {ErrorMessage}", uuid, unsigned, ex);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "An error occurred while processing the request: " + ex.Message);
        }
    }

    private async Task<string> GetProfileResponseJsonAsync(CustomUser user, bool unsigned = true)
    {
        if (_profileCache.TryGet(user.Id, out var profile))
            return profile;

        Dictionary<string, object> textures = new  Dictionary<string, object>();
        var skin = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
        if (skin != null)
        {
            textures.Add("skin", new Dictionary<string, object>
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

        var cape = _dbContext.FindUserCape(x => x.UserId == user.Id && x.IsSelected)?.Cape.FileData;
        if (cape != null)
        {
            textures.Add("cape", new Dictionary<string, object>
            {
                { "url", cape.GetUrl(_settings.ApiUrl) },
            });
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

        List<Dictionary<string, object>> properties = [];
        Dictionary<string, object> texturesProperty = new Dictionary<string, object>
        {
            { "name", "textures" },
            { "value", Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(textureValues, Formatting.None))) },
        };
        if (!unsigned)
        {
            var cert = X509CertificateLoader.LoadPkcs12(_settings.Cert, "");
            var rsa = cert.GetRSAPrivateKey();
            if (rsa != null)
                texturesProperty.Add("signature", rsa.ExportSubjectPublicKeyInfoPem());
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