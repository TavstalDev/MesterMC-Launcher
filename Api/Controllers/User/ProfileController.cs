using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/profile")]
public class ProfileController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    // TODO: Add following endpoints:
    // - GET /profile: Get the current user's profile information.
    // - PATCH /profile: Update the current user's profile information (e.g., username, email, avatar).
    // - POST /profile/avatar: Upload a new avatar for the current user.
    // - DELETE /profile/avatar: Remove the current user's avatar.
    // - GET /profile/avatar: Get the current user's avatar.
    // - GET /profile/security: Get the current user's security settings (e.g., 2FA status).
    // - PATCH /profile/security: Update the current user's security settings (e.g., enable/disable 2FA).
    // - GET /profile/sessions: Get a list of the current user's active sessions.
    // - DELETE /profile/sessions/{id}: Terminate a specific active session for the current user.
    
    public ProfileController(ILogger<ProfileController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
}