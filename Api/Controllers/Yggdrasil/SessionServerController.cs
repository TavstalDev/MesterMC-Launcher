using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

[Route("/yggdrasil/sessionserver/session/minecraft")]
public class SessionServerController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public SessionServerController(IConfiguration configuration, ILogger<SessionServerController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }
    
    [HttpPost("join")]
    public IActionResult Join([FromBody] YigJoinServerRequest request)
    {
        return this.ReturnResponseCode(HttpStatusCode.NoContent);
    }

    [HttpGet("hasJoined")]
    public IActionResult HasJoined([FromQuery] string serverId, [FromQuery] string username, [FromQuery] string? ip)
    {
        /*
         * EXAMPLE RESPONSE
         *
         * {
             "id": "5f8d0d55a3b24f2ab9c6e4fcd1234567",
             "name": "PlayerOne",
             "properties": [
               {
                 "name": "textures",
                 "value": "BASE64_TEXTURES_JSON",
                 "signature": "BASE64_SIGNATURE"
               }
             ]
           }
           
         *
         */
        return Ok();
    }

    [HttpGet("profile/{uuid}")]
    public IActionResult GetProfile(string uuid, [FromQuery] bool? unsigned)
    {
        /*
         * EXAMPLE RESPONSE
         *
         * {
             "id": "5f8d0d55a3b24f2ab9c6e4fcd1234567",
             "name": "PlayerOne",
             "properties": [
               {
                 "name": "textures",
                 "value": "BASE64_TEXTURES_JSON",
                 "signature": "BASE64_SIGNATURE"
               }
             ]
           }
           
         *
         */
        return Ok();
    }
}