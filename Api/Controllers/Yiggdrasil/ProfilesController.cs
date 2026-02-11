using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Yiggdrasil;

[Route("/yiggdrasil/api/profiles")]
public class ProfilesController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public ProfilesController(IConfiguration configuration, ILogger<ProfilesController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpPost("minecraft")]
    public IActionResult MinecraftProfile([FromBody] List<String> names)
    {
        /*
         * EXAMPLE RESPONSE
         *
         * [
             {
               "id": "5f8d0d55a3b24f2ab9c6e4fcd1234567",
               "name": "PlayerOne"
             },
             {
               "id": "8a7b6c5d4e3f2a1b0c9d8e7f12345678",
               "name": "AnotherPlayer"
             }
           ]
           
         *
         */
        return Ok();
    }
}