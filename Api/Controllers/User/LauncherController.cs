using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/launcher")]
public class LauncherController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public LauncherController(ILogger<LauncherController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
}