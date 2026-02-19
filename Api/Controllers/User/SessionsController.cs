using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class SessionsController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    // TODO: Add following endpoints:
    // - GET /user/sessions: Get a list of active sessions for the current user.
    // - DELETE /user/sessions/{sessionId}: Revoke a specific session by its ID (log out from that session).
    // - DELETE /user/sessions: Revoke all sessions for the current user (log out from all devices).
    // - GET /user/{id}/sessions: Get a list of active sessions for a specific user by their ID (admin only).
    // - DELETE /user/{id}/sessions/{sessionId}: Revoke a specific session for a specific user by their ID (admin only).
    // - DELETE /user/{id}/sessions: Revoke all sessions for a specific user by their ID (admin only).
    
    public SessionsController(ILogger<SessionsController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
}