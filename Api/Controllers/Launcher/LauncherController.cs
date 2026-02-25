using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models.Database.Launcher;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Launcher;

[ApiController]
[Route("/launcher")]
public class LauncherController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly MemoryCacheService _memoryCacheService;
    private readonly TimeSpan CacheTTL = TimeSpan.FromHours(1);
    
    // TODO: Add following endpoints:
    // - POST /launcher/version: Add a new launcher version (admin only).
    // - DELETE /launcher/version/{id}: Remove a launcher version (admin only).
    // - PATCH /launcher/version/{id}: Update details of a specific launcher version (admin only).
    
    public LauncherController(ILogger<LauncherController> logger, CustomUserManager userManager, CustomDbContext dbContext, MemoryCacheService memoryCacheService) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _memoryCacheService = memoryCacheService;
    }

    [HttpGet("versions")]
    public async Task<IActionResult> GetLauncherVersions()
    {
        var versions = await _dbContext.GetLauncherVersionsAsync();
        if (!versions.Any())
            return ReturnResponseCode(HttpStatusCode.NotFound, "No launcher versions found.");
        
        return ReturnJson(versions);
    }

    [HttpGet("versions/latest")]
    public async Task<IActionResult> GetLatestLauncherVersion()
    {
        var version = await _dbContext.FindLatestLauncherVersionAsync();
        if (version == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No launcher versions found.");
        return ReturnJson(version);
    }

    [HttpGet("version/{id}")]
    public async Task<IActionResult> GetLauncherVersionDetails([BindRequired, FromRoute] ulong id)
    {
        var version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
        if (version == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");
        return ReturnJson(version);
    }

    [HttpGet("version/{id}/download")]
    public async Task<IActionResult> GetLauncherVersionDownloadLink([BindRequired, FromRoute] ulong id, [BindRequired, FromQuery] ELauncherOs os)
    {
        string cacheKey = $"launcher_version:{id}:download_{os}";
        if (_memoryCacheService.TryGetValue(cacheKey, out (byte[], string) cachedVersion))
            return File(cachedVersion.Item1, cachedVersion.Item2);
        
        var version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
        if (version == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");
        
        var versionData = await _dbContext.FindLauncherVersionDataAsync(x => x.Os == os && x.VersionId == version.Id);
        if (versionData == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version data not found.");

        var fileData = await _dbContext.FindFileDataAsync(x => x.Id == versionData.FileId);
        if (fileData == null || !fileData.Exists())
            return  ReturnResponseCode(HttpStatusCode.NotFound, "File data not found.");

        byte[]? bytes = fileData.GetFileData();
        if (bytes == null)
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "Failed to retrieve the file.");
        
        _memoryCacheService.SetValue(cacheKey, (bytes, fileData.ContentType), CacheTTL);
        return File(bytes, fileData.ContentType);
    }

    #region Admin Endpoints
    
    #endregion
}