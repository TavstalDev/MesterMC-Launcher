using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

[ApiController]
[Route("yggdrasil")]
[Tags("Yggdrasil")]
public class StatusController : Controller
{
    private readonly ILogger _logger;
    private readonly Settings _settings;
    
    public StatusController(ILogger<StatusController> logger, Settings settings)
    {
        _logger = logger;
        _settings = settings;
    }
    
    [HttpGet] 
    public IActionResult Root()
    {
        var cert = X509CertificateLoader.LoadPkcs12(_settings.Cert, _settings.CertPassword);
        var rsa = cert.GetRSAPrivateKey();
        if (rsa == null)
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Failed to load RSA private key from certificate");

        string signature = rsa.ExportSubjectPublicKeyInfoPem();
        return this.ReturnJson(new
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

    [HttpGet("status")]
    public IActionResult Status()
    {
        // TODO: Implement actual status checks for users, tokens, and pending authentications
        return this.ReturnJson(new Dictionary<string, object>
        {
            { "user.count", 0 },
            { "token.count", 0 },
            { "pendingAuthentication.count", 0 }
        });
    }
    
    [HttpGet("publickeys")]
    [HttpGet("minecraftservices/publickeys")]
    public IActionResult GetPublicKeys()
    {
        return this.ReturnJson(new { profileKeys = new object[] { } });
    }
}