using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Yiggdrasil;

[Route("/yiggdrasil/textures")]
public class TexturesController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public TexturesController(IConfiguration configuration, ILogger<TexturesController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }
    
    [HttpGet("{hash}")]
    public IActionResult GetTexture(string hash)
    {
        // Should return the file if exists
        return Ok();
    }
}