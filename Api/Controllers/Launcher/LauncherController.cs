using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models.Bodies.Launcher;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.Launcher;
using Tavstal.MesterMC.Api.Models.Database.User;
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

    [HttpPost("version")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    public async Task<IActionResult> CreateLauncherVersion([BindRequired, FromForm] CreateLauncherVersionRequest request)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.Launcher.CreateVersion))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

        LauncherVersion? existingVersion = await _dbContext.FindLauncherVersionAsync(x => x.Version == request.Version);
        if (existingVersion != null)
            return ReturnResponseCode(HttpStatusCode.BadRequest, "A launcher version with the same version number already exists.");
        
        await _dbContext.AddLauncherVersionAsync(new LauncherVersion
        {
            Version = request.Version,
            VersionType = request.VersionType,
            Changelog = request.Changelog,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        }, true);
        
        return ReturnResponseCode(HttpStatusCode.OK, "Launcher version created successfully.");
    }

    [HttpPut("version/{id}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    public async Task<IActionResult> UpdateLauncherVersion([BindRequired, FromRoute] ulong id, [BindRequired, FromForm] UpdateLauncherVersionRequest request)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.Launcher.UpdateVersion))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");
        
        LauncherVersion? version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
        if (version == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");

        if (!string.IsNullOrEmpty(request.Version))
        {
            LauncherVersion? existingVersion = await _dbContext.FindLauncherVersionAsync(x => x.Version == request.Version && x.Id != version.Id);
            if (existingVersion != null)
                return ReturnResponseCode(HttpStatusCode.BadRequest, "A launcher version with the same version number already exists.");
            
            version.Version = request.Version;
        }
        
        if (!string.IsNullOrEmpty(request.Changelog))
            version.Changelog = request.Changelog;
        
        if (request.VersionType != null)
            version.VersionType = request.VersionType.Value;

        await _dbContext.UpdateLauncherVersionAsync(version, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Launcher version updated successfully.");
    }

    [HttpDelete("version/{id}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    public async Task<IActionResult> DeleteLauncherVersion([BindRequired, FromRoute] ulong id)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.Launcher.DeleteVersion))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");
        
        LauncherVersion? version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
        if (version == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");
        
        await _dbContext.ClearLauncherVersionDatasAsync(version.Id);
        await _dbContext.RemoveLauncherVersionAsync(version, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Launcher version deleted successfully.");
    }

    [HttpPost("version/{id}/data")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    public async Task<IActionResult> AddLauncherVersionData([BindRequired, FromRoute] ulong id, [BindRequired, FromForm] CreateLauncherVersionDataRequest request)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.Launcher.CreateVersion))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");
        
        LauncherVersion? version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
        if (version == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");
        
        LauncherVersionData? versionData = await _dbContext.FindLauncherVersionDataAsync(x => x.VersionId == version.Id && x.Os == request.Os);
        if (versionData != null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version data for the specified OS already exists.");

        if (request.File.Length > 1024 * 1024 * 512) // 512 MB limit
            return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 512 MB limit.");
        
        if (!(request.File.FileName.EndsWith(".zip") || request.File.FileName.EndsWith(".tar.gz") || request.File.FileName.EndsWith(".tar")))
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid file type. Only .zip, .tar.gz, and .tar files are allowed.");

        await using var stream = request.File.OpenReadStream();
        using var sha256 = SHA256.Create();
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        string fileHash = Convert.ToHexStringLower(hashBytes);
        stream.Position = 0;
        
        FileData fd = await _dbContext.AddFileDataAsync(new FileData
        {
            Hash = fileHash,
            FileName = $"{Guid.NewGuid():N}.{request.File.FileName.Split('.').Last()}",
            ContentType = request.File.ContentType,
            Type = EFileDataType.LAUNCHER
        }, true);
        fd.SaveFile(stream);

        await _dbContext.AddLauncherVersionDataAsync(new LauncherVersionData
        {
            VersionId = version.Id,
            FileId = fd.Id,
            Os = request.Os,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        }, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Launcher version data added successfully.");
    }

    [HttpDelete("version/{versionId}/data/{dataId}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    public async Task<IActionResult> DeleteLauncherVersionData([BindRequired, FromRoute] ulong versionId, [BindRequired, FromRoute] ulong dataId)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.Launcher.DeleteVersion))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");
        
        LauncherVersionData? versionData = await _dbContext.FindLauncherVersionDataAsync(x => x.Id == dataId && x.VersionId == versionId);
        if (versionData == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version data not found.");
        
        FileData? fileData = await _dbContext.FindFileDataAsync(x => x.Id == versionData.FileId);
        if (fileData != null)
        {
            fileData.DeleteFile();
            await _dbContext.RemoveFileDataAsync(fileData);
        }
            
        await _dbContext.RemoveLauncherVersionDataAsync(versionData, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Launcher version data added successfully.");
    }
    #endregion
}