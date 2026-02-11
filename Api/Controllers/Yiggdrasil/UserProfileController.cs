using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yiggdrasil;

[Route("/yiggdrasil/api/user/profile")]
public class UserProfileController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public UserProfileController(IConfiguration configuration, ILogger<UserProfileController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpDelete("{uuid}/{textureType}")]
    public IActionResult DeleteTexture(string uuid, string textureType)
    {
        return this.ReturnResponseCode(HttpStatusCode.NoContent);
    }
    
    [HttpPut("{uuid}/{textureType}")]
    public IActionResult UpdateTexture(string uuid, string textureType)
    {
        return this.ReturnResponseCode(HttpStatusCode.NoContent);
    }
}