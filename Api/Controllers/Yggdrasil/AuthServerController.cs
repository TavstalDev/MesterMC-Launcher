using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

[Route("/yggdrasil/authserver")]
public class AuthServerController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public AuthServerController(IConfiguration configuration, ILogger<AuthServerController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpPost("authenticate")]
    public IActionResult Authenticate([FromBody] YigLoginRequest request)
    {
        /*
         * EXAMPLE RESPONSE
         *
         * {
             "accessToken": "d3f8a4b9-9b8f-4e1a-8e3a-4d1c9e2b5f77",
             "clientToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
             "availableProfiles": [
               {
                 "id": "5f8d0d55a3b24f2ab9c6e4fcd1234567",
                 "name": "PlayerOne"
               }
             ],
             "selectedProfile": {
               "id": "5f8d0d55a3b24f2ab9c6e4fcd1234567",
               "name": "PlayerOne"
             },
             "user": {
               "id": "c1a2b3d4e5f67890abcd1234567890ef",
               "properties": []
             }
           }
         * 
         */
        return Ok();
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] YigRefreshRequest request)
    {
        /*
         * EXAMPLE RESPONSE
         *
         * {
             "accessToken": "d3f8a4b9-9b8f-4e1a-8e3a-4d1c9e2b5f77",
             "clientToken": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
             "selectedProfile": {
               "id": "5f8d0d55a3b24f2ab9c6e4fcd1234567",
               "name": "PlayerOne"
             },
             "user": {
               "id": "c1a2b3d4e5f67890abcd1234567890ef",
               "properties": []
             }
           }
           
         * 
         */
        return Ok();
    }

    [HttpPost("validate")]
    public IActionResult Validate([FromBody] YigValidateRequest request)
    {
        return this.ReturnResponseCode(HttpStatusCode.NoContent);
    }

    [HttpPost("invalidate")]
    public IActionResult Invalidate([FromBody] YigValidateRequest request)
    {
        return this.ReturnResponseCode(HttpStatusCode.NoContent);
    }

    [HttpPost("signout")]
    public IActionResult SignOut([FromBody] YigSignoutRequest request)
    {
        return this.ReturnResponseCode(HttpStatusCode.NoContent);
    }
}