using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class AvatarController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    // TODO: Add following endpoints:
    // - POST /user/avatar: Upload a new avatar for the current user.
    // - DELETE /user/avatar: Remove the current user's avatar.
    // - GET /user/avatar: Get the current user's avatar.
    // - GET /user/{id}/avatar: Get a specific user's avatar by their ID.
    // - POST /user/{id}/avatar: Upload a new avatar for a specific user by their ID (admin only).
    // - DELETE /user/{id}/avatar: Remove a specific user's avatar by their ID (admin only).
    
    public AvatarController(ILogger<AvatarController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
    
}