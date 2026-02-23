using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Launcher;

[Route("/launcher")]
public class LauncherController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    // TODO: Add following endpoints:
    // - GET /launcher/versions: Get a list of available launcher versions.
    // - GET /launcher/versions/latest: Get the latest launcher version.
    // - POST /launcher/version: Add a new launcher version (admin only).
    // - DELETE /launcher/version/{id}: Remove a launcher version (admin only).
    // - GET /launcher/version/{id}: Get details of a specific launcher version.
    // - PATCH /launcher/version/{id}: Update details of a specific launcher version (admin only).
    // - GET /launcher/version/{id}/download: Get a download link for a specific launcher version.
    // - GET /launcher/version/{id}/changelog: Get the changelog for a specific launcher version.
    
    public LauncherController(ILogger<LauncherController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
}