using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/launcher")]
public class LauncherController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public LauncherController(IConfiguration configuration, ILogger<LauncherController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }
}