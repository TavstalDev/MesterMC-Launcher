using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

/// <summary>
/// Controller for handling Yggdrasil status-related API requests.
/// </summary>
[ApiController]
[Route("yggdrasil")]
[Tags("Yggdrasil")]
public class StatusController : CustomControllerBase
{
    private readonly Settings _settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StatusController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="settings">The application settings.</param>
    public StatusController(ILogger<StatusController> logger, Settings settings) : base(logger)
    {
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
        var cert = X509CertificateLoader.LoadPkcs12(_settings.Cert, _settings.CertPassword);
        var rsa = cert.GetRSAPrivateKey();
        if (rsa == null)
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "Failed to load RSA private key from certificate");

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

    /// <summary>
    /// Retrieves the current status of users, tokens, and pending authentications.
    /// </summary>
    /// <returns>
    /// A JSON response containing the status counts.
    /// </returns>
    /// <response code="200">Returns the status counts.</response>
    [HttpGet("status")]
    public IActionResult Status()
    {
        // TODO: Implement actual status checks for users, tokens, and pending authentications
        return ReturnJson(new Dictionary<string, object>
        {
            { "user.count", 0 },
            { "token.count", 0 },
            { "pendingAuthentication.count", 0 }
        });
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
        return ReturnJson(new { profileKeys = new object[] { } });
    }
}