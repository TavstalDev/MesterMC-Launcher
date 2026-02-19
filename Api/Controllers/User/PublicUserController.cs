using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/profile")]
public class PublicUserController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    // TODO: Add following endpoints:
    // - GET /profile: Get the current user's profile information.
    // - PATCH /profile: Update the current user's profile information (e.g., username, email, avatar).
    
    public PublicUserController(ILogger<PublicUserController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
}