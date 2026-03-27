using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

/// <summary>
/// Controller for handling Yggdrasil status-related API requests.
/// </summary>
[ApiController]
[Route("yggdrasil")]
[Tags("Yggdrasil")]
public class StatusController : CustomControllerBase
{
    private readonly CustomDbContext _dbContext;
    private readonly CustomUserManager _userManager;
    private readonly MemoryCacheService _cacheService;
    private readonly Settings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="StatusController"/> class.
    /// </summary>
    /// <param name="cacheService">Service for caching data in memory.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="settings">The application settings.</param>
    /// <param name="userManager">Service for managing users.</param>
    /// <param name="dbContext">Database context for accessing data.</param>
    public StatusController(CustomDbContext dbContext, CustomUserManager userManager, MemoryCacheService cacheService, ILogger<StatusController> logger, Settings settings) : base(logger, settings)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _cacheService = cacheService;
        _settings = settings;
    }
    
    /// <summary>
    /// Retrieves the root status information, including skin domains, public key signature, and metadata.
    /// </summary>
    /// <returns>
    /// A JSON response containing the root status information.
    /// </returns>
    /// <response code="200">Returns the root status information.</response>
    /// <response code="500">Failed to load the RSA private key from the certificate.</response>
    [HttpGet] 
    public IActionResult Root()
    {
        try
        {
            var cert = X509CertificateLoader.LoadPkcs12(_settings.Cert, _settings.CertPassword);
            var rsa = cert.GetRSAPrivateKey();
            if (rsa == null)
                return ReturnResponseCode(HttpStatusCode.InternalServerError,
                    "Failed to load RSA private key from certificate");

            string signature = rsa.ExportSubjectPublicKeyInfoPem();
            return ReturnJson(new
            {
                skinDomains = _settings.SkinDomains,
                signaturePublickey = signature,
                meta = new Dictionary<string, object>
                {
                    { "serverName", _settings.ServerName },
                    { "implementationVersion", _settings.ImplementationVersion },
                    { "feature.non_email_login", true },
                    { "implementationName", _settings.ImplementationName }
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving yggdrasil status");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Retrieves the current status of users, tokens, and pending authentications.
    /// </summary>
    /// <returns>
    /// A JSON response containing the status counts.
    /// </returns>
    /// <response code="200">Returns the status counts.</response>
    [HttpGet("status")]
    public Task<IActionResult> Status()
    {
        if (_cacheService.TryGetValue("yggdrasil_status", out string? cachedResult) && !string.IsNullOrEmpty(cachedResult))
            return Task.FromResult(ReturnJson(cachedResult));
        
        try
        {
            var result = JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                { "user.count", _userManager.Users.Count() },
                { "token.count", _dbContext.UserTokens.Count() },
                { "pendingAuthentication.count", 0 } // Not implemented
            }, Formatting.Indented);
            _cacheService.SetValue("yggdrasil_status", result, TimeSpan.FromMinutes(5));
            return Task.FromResult(ReturnJson(result));
        }
        catch (Exception exception)
        {
            return Task.FromException<IActionResult>(exception);
        }
    }
    
    /// <summary>
    /// Retrieves the public keys for profile verification.
    /// </summary>
    /// <returns>
    /// A JSON response containing an empty list of profile keys.
    /// </returns>
    /// <response code="200">Returns an empty list of profile keys.</response>
    [HttpGet("publickeys")]
    [HttpGet("minecraftservices/publickeys")]
    public IActionResult GetPublicKeys()
    {
        return ReturnJson(new { profileKeys = Array.Empty<object>() });
    }
}