using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yiggdrasil;

[Route("/yiggdrasil")]
public class StatusController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    
    public StatusController(IConfiguration configuration, ILogger<StatusController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    
    [HttpGet("")] 
    public IActionResult Root()
    {
        return this.ReturnJson(HttpStatusCode.OK, new
        {
            skinDomains = new[]
            {
                "localhost"
            },
            signaturePublicKey = "", // TODO: Add a public key for signature verification
            meta = new Dictionary<string, object>
            {
                { "serverName", "MMC's yiggdrasil server" },
                { "implementationVersion", "1.0.0" },
                { "feature.non_email_login", true },
                { "implementationName", "yiggdrasil-mock-server" }
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