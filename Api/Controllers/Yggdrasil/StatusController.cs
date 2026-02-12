using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

[Route("/yggdrasil")]
public class StatusController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly Settings _settings;
    
    public StatusController(IConfiguration configuration, ILogger<StatusController> logger, Settings settings)
    {
        _configuration = configuration;
        _logger = logger;
        _settings = settings;
    }
    
    [HttpGet()] 
    public IActionResult Root()
    {
        var signature =  X509CertificateLoader.LoadCertificate(_settings.PfxCert).GetRSAPrivateKey()!.ExportSubjectPublicKeyInfoPem();
        return this.ReturnJson(HttpStatusCode.OK, new
        {
            skinDomains = _settings.SkinDomains,
            signaturePublicKey =signature,
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
        return this.ReturnJson(HttpStatusCode.OK, new Dictionary<string, object>
        {
            { "user.count", 0 },
            { "token.count", 0 },
            { "pendingAuthentication.count", 0 }
        });
    }
}