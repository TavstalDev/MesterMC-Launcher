using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database.Server;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
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
    private readonly MemoryCacheService _cacheService;
    private static readonly TimeSpan SignedTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan UnsignedTtl = TimeSpan.FromHours(1);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionServerController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="userManager">The user manager for handling user-related operations.</param>
    /// <param name="dbContext">The database context for accessing data.</param>
    /// <param name="cacheService">The memory cache service for caching data.</param>
    /// <param name="settings">The application settings.</param>
    public SessionServerController(ILogger<SessionServerController> logger, CustomUserManager userManager, CustomDbContext dbContext, MemoryCacheService cacheService, Settings settings) : base(logger, settings)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _settings = settings;
        _cacheService = cacheService;
    }
    
    /// <summary>
    /// Retrieves the list of blocked servers.
    /// </summary>
    /// <returns>A JSON response containing an empty list of blocked servers.</returns>
    [HttpGet("/yggdrasil/sessionserver/blockedservers")]
    public IActionResult GetBlockedServers()
    {
        // Until no actual blocked servers are implemented, it does not need to be cached or retrieved from the database, so we can just return an empty list with the correct structure.
        string finalJson = JsonConvert.SerializeObject(new
        {
            blockedServers = Array.Empty<string>()
        });
        string etag = ComputeETag(finalJson);
        if (HttpContext.Request.Headers.TryGetValue("If-None-Match", out var incomingEtag))
        {
            if (incomingEtag.ToString().Equals(etag, StringComparison.Ordinal))
            {
                HttpContext.Response.Headers.ETag = etag;
                return ReturnResponseCode(HttpStatusCode.NotModified);
            }
        }

        HttpContext.Response.Headers.ETag = etag;
        return ReturnJson(finalJson);
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
    public async Task<IActionResult> Join([Required, FromBody] YigJoinServerRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            string dashedUuid = Guid.Parse(request.selectedProfile).ToString("D");
            CustomUser? user = await _userManager.FindByIdAsync(dashedUuid);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound,
                    "User not found for the provided selectedProfile UUID");

            if (!await _userManager.VerifyJwtTokenAsync(request.accessToken))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid access token");
            
            UserPlaySession? session = await _dbContext.FindUserPlaySessionAsync(x => x.Token == request.accessToken);
            if (session == null)
                return ReturnResponseCode(HttpStatusCode.NotFound,
                    "No active session found for the provided access token");

            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (now > session.ExpiresAt)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "The access token has expired");

            string host = HttpContext.Request.Host.Host;
            if (session.UserIp != host)
                return ReturnResponseCode(HttpStatusCode.Forbidden,
                    "The IP address associated with the access token does not match the IP address of the request");

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
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Unknown error while processing join request for selectedProfile: {SelectedProfile}, serverId: {ServerId}.", request.selectedProfile, request.serverId);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
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
    public async Task<IActionResult> HasJoined([FromQuery] string serverId, [FromQuery, MinLength(3), MaxLength(16)] string username, [FromQuery, MinLength(7), MaxLength(15)] string? ip)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            CustomUser? user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found");

            ServerJoin? join =
                await _dbContext.FindServerJoinAsync(x => x.ServerId == serverId && (x.UserIp == ip || ip == null));
            if (join == null)
                return ReturnResponseCode(HttpStatusCode.NotFound,
                    "No matching server join found for the provided serverId and IP address");

            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (now > join.ExpiresAt)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "The server join has expired");

            if (user.Id != join.UserId)
                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    "The provided username does not match the user associated with the server join");

            string json = await GetProfileResponseJsonAsync(user);
            return ReturnJson(json);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Unknown error while processing hasJoined request for serverId: {ServerId}, username: {Username}.", serverId, username);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
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
    public async Task<IActionResult> GetProfile([BindRequired, FromRoute, MinLength(32), MaxLength(36)] string uuid, [FromQuery] bool unsigned = true)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest, string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }
            
            string dashedUuid = Guid.Parse(uuid).ToString("D");
            CustomUser? user = await _userManager.FindByIdAsync(dashedUuid);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found");

            string json = await GetProfileResponseJsonAsync(user, unsigned);
            string etag = ComputeETag(json);
            if (HttpContext.Request.Headers.TryGetValue("If-None-Match", out var incomingEtag))
            {
                if (incomingEtag.ToString().Equals(etag, StringComparison.Ordinal))
                {
                    HttpContext.Response.Headers.ETag = etag;
                    return ReturnResponseCode(HttpStatusCode.NotModified);
                }
            }

            HttpContext.Response.Headers.ETag = etag;
            return ReturnJson(json);
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Unknown error while processing profile request for uuid: {Uuid}, unsigned: {Unsigned}. Error: {ErrorMessage}", uuid, unsigned, ex);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Generates or retrieves cached profile JSON for a given user.
    /// The method uses an in-memory cache with different TTLs for signed and unsigned profiles.
    /// A per-user semaphore is used to prevent cache stampedes.
    /// </summary>
    /// <param name="user">The user for whom the profile JSON is being generated.</param>
    /// <param name="unsigned">
    /// A boolean indicating whether the profile should be unsigned. 
    /// Defaults to true.
    /// </param>
    /// <returns>A JSON string representing the user's profile.</returns>
    private async Task<string> GetProfileResponseJsonAsync(CustomUser user, bool unsigned = true)
    {
        string key = $"profile:{user.Id}:{(unsigned ? "unsigned" : "signed")}";
        TimeSpan ttl = unsigned ? UnsignedTtl : SignedTtl;
        
        if (_cacheService.TryGetValue<string>(key, out var cached))
            return cached!;

        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            // double-check after acquiring lock
            if (_cacheService.TryGetValue<string>(key, out var cachedAfterLock))
                return cachedAfterLock!;
            Dictionary<string, object> textures = new Dictionary<string, object>();
            var skin = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
            if (skin != null)
            {
                textures.Add("SKIN", new Dictionary<string, object>
                {
                    { "url", skin.GetUrl(_settings.ApiUrl, true) },
                    {
                        "metadata", new Dictionary<string, object>
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
                            { "url", capeData.GetUrl(_settings.ApiUrl, true) },
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
            string finalJson = JsonConvert.SerializeObject(response, Formatting.None);
            
            // Cache the result with absolute expiration
            _cacheService.SetValue(key, finalJson, ttl);
            return finalJson;
        }
        finally
        {
            sem.Release();
        }
    }
}