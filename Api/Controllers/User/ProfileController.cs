using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/profile")]
public class ProfileController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public ProfileController(ILogger<ProfileController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
}