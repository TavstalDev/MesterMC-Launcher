using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Bodies.Launcher;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.Launcher;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Launcher;

/// <summary>
/// Controller for managing launcher versions and their associated data.
/// </summary>
[ApiController]
[Route("/launcher")]
public class LauncherController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly MemoryCacheService _memoryCacheService;
    private readonly TimeSpan CacheTTL = TimeSpan.FromHours(1);
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="userManager">Custom user manager for user operations.</param>
    /// <param name="dbContext">Database context for accessing launcher data.</param>
    /// <param name="memoryCacheService">Service for caching launcher data.</param>
    /// <param name="settings">Application settings.</param>
    public LauncherController(ILogger<LauncherController> logger, CustomUserManager userManager, CustomDbContext dbContext, MemoryCacheService memoryCacheService, Settings settings) : base(logger, settings)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _memoryCacheService = memoryCacheService;
    }

    /// <summary>
    /// Retrieves all launcher versions.
    /// </summary>
    /// <response code="200">Returns a list of launcher versions.</response>
    /// <response code="404">No launcher versions found.</response>
    [HttpGet("versions")]
    [JsonResponse(typeof(List<LauncherVersion>)), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLauncherVersions()
    {
        try
        {
            var versions = await _dbContext.GetLauncherVersionsAsync();
            if (versions.Count == 0)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No launcher versions found.");

            return ReturnJson(versions);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while retrieving launcher versions.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Retrieves the latest launcher version.
    /// </summary>
    /// <response code="200">Returns the latest launcher version.</response>
    /// <response code="404">No launcher versions found.</response>
    [HttpGet("versions/latest")]
    [JsonResponse(typeof(LauncherVersion)), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestLauncherVersion()
    {
        try
        {
            var version = await _dbContext.FindLatestLauncherVersionAsync();
            if (version == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No launcher versions found.");
            return ReturnJson(version);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while retrieving the latest launcher version.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Retrieves details of a specific launcher version by ID.
    /// </summary>
    /// <param name="id">The ID of the launcher version.</param>
    /// <response code="200">Returns the launcher version details.</response>
    /// <response code="404">Launcher version not found.</response>
    [HttpGet("version/{id}")]
    [EnableRateLimiting(RateLimits.SEARCH)]
    [JsonResponse(typeof(LauncherVersion)), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLauncherVersionDetails([BindRequired, FromRoute] ulong id)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            var version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
            if (version == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");
            
            var versionDetails = await _dbContext.GetLauncherVersionDatasAsync(x => x.VersionId == version.Id);
            return ReturnJson(versionDetails);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while retrieving launcher version details.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Retrieves the download link for a specific launcher version and OS.
    /// </summary>
    /// <param name="id">The ID of the launcher version.</param>
    /// <param name="os">The operating system for the launcher version.</param>
    /// <response code="200">Returns the file to download.</response>
    /// <response code="404">Launcher version or file data not found.</response>
    /// <response code="500">Failed to retrieve the file.</response>
    [HttpGet("version/{id}/download")]
    [EnableRateLimiting(RateLimits.DOWNLOAD)]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DownloadLauncherVersion([BindRequired, FromRoute] ulong id, [BindRequired, FromQuery] ELauncherOs os)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            string cacheKey = $"launcher_version:{id}:download_{os}";
            if (_memoryCacheService.TryGetValue(cacheKey, out (byte[], string) cachedVersion))
                return File(cachedVersion.Item1, cachedVersion.Item2);

            var version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
            if (version == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");

            var versionData =
                await _dbContext.FindLauncherVersionDataAsync(x => x.Os == os && x.VersionId == version.Id);
            if (versionData == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version data not found.");

            var fileData = await _dbContext.FindFileDataAsync(x => x.Id == versionData.FileId);
            if (fileData == null || !fileData.Exists())
                return ReturnResponseCode(HttpStatusCode.NotFound, "File data not found.");

            byte[]? bytes = fileData.GetFileData();
            if (bytes == null)
                return ReturnResponseCode(HttpStatusCode.InternalServerError, "Failed to retrieve the file.");

            _memoryCacheService.SetValue(cacheKey, (bytes, fileData.ContentType), CacheTTL);
            return File(bytes, fileData.ContentType);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while retrieving the launcher version download link.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    #region Admin Endpoints

    /// <summary>
    /// Creates a new launcher version.
    /// </summary>
    /// <param name="request">The request body containing launcher version details.</param>
    /// <response code="200">Launcher version created successfully.</response>
    /// <response code="400">Invalid request or duplicate version.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions.</response>
    [HttpPost("version")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [Consumes("application/json")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateLauncherVersion([Required, FromBody] CreateLauncherVersionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!_userManager.HasPermission(user, CustomPermissions.Launcher.CreateVersion))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Permission denied.");

            LauncherVersion? existingVersion =
                await _dbContext.FindLauncherVersionAsync(x => x.Version == request.Version);
            if (existingVersion != null)
                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    "A launcher version with the same version number already exists.");

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
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while creating a new launcher version.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Updates an existing launcher version.
    /// </summary>
    /// <param name="id">The ID of the launcher version to update.</param>
    /// <param name="request">The request body containing updated launcher version details.</param>
    /// <response code="200">Launcher version updated successfully.</response>
    /// <response code="400">Invalid request or duplicate version.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions.</response>
    /// <response code="404">Launcher version not found.</response>
    [HttpPut("version/{id}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [Consumes("application/json")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLauncherVersion([BindRequired, FromRoute] ulong id, [Required, FromBody] UpdateLauncherVersionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!_userManager.HasPermission(user, CustomPermissions.Launcher.UpdateVersion))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Permission denied.");

            LauncherVersion? version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
            if (version == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");

            if (!string.IsNullOrEmpty(request.Version))
            {
                LauncherVersion? existingVersion =
                    await _dbContext.FindLauncherVersionAsync(x => x.Version == request.Version && x.Id != version.Id);
                if (existingVersion != null)
                    return ReturnResponseCode(HttpStatusCode.BadRequest,
                        "A launcher version with the same version number already exists.");

                version.Version = request.Version;
            }

            if (!string.IsNullOrEmpty(request.Changelog))
                version.Changelog = request.Changelog;

            if (request.VersionType != null)
                version.VersionType = request.VersionType.Value;

            await _dbContext.UpdateLauncherVersionAsync(version, true);
            return ReturnResponseCode(HttpStatusCode.OK, "Launcher version updated successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while updating the launcher version.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Deletes a launcher version.
    /// </summary>
    /// <param name="id">The ID of the launcher version to delete.</param>
    /// <response code="200">Launcher version deleted successfully.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions.</response>
    /// <response code="404">Launcher version not found.</response>
    [HttpDelete("version/{id}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLauncherVersion([BindRequired, FromRoute] ulong id)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!_userManager.HasPermission(user, CustomPermissions.Launcher.DeleteVersion))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Permission denied.");

            LauncherVersion? version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
            if (version == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");

            await _dbContext.ClearLauncherVersionDatasAsync(version.Id);
            await _dbContext.RemoveLauncherVersionAsync(version, true);
            return ReturnResponseCode(HttpStatusCode.OK, "Launcher version deleted successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while deleting the launcher version.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Adds data for a specific launcher version.
    /// </summary>
    /// <param name="id">The ID of the launcher version.</param>
    /// <param name="request">The request body containing launcher version data details.</param>
    /// <response code="200">Launcher version data added successfully.</response>
    /// <response code="400">Invalid request or file type.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions.</response>
    /// <response code="404">Launcher version not found.</response>
    [HttpPost("version/{id}/data")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [Consumes("multipart/form-data")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddLauncherVersionData([BindRequired, FromRoute] ulong id, [Required, FromForm] CreateLauncherVersionDataRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!_userManager.HasPermission(user, CustomPermissions.Launcher.CreateVersion))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Permission denied.");

            LauncherVersion? version = await _dbContext.FindLauncherVersionAsync(x => x.Id == id);
            if (version == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Launcher version not found.");

            LauncherVersionData? versionData =
                await _dbContext.FindLauncherVersionDataAsync(x => x.VersionId == version.Id && x.Os == request.Os);
            if (versionData != null)
                return ReturnResponseCode(HttpStatusCode.NotFound,
                    "Launcher version data for the specified OS already exists.");

            if (request.File.Length > 1024 * 1024 * 512) // 512 MB limit
                return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 512 MB limit.");

            if (!(request.File.FileName.EndsWith(".zip") || request.File.FileName.EndsWith(".tar.gz") ||
                  request.File.FileName.EndsWith(".tar")))
                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    "Invalid file type. Only .zip, .tar.gz, and .tar files are allowed.");

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
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while adding launcher version data.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Deletes data for a specific launcher version.
    /// </summary>
    /// <param name="versionId">The ID of the launcher version.</param>
    /// <param name="dataId">The ID of the launcher version data to delete.</param>
    /// <response code="200">Launcher version data deleted successfully.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">Insufficient permissions.</response>
    /// <response code="404">Launcher version data not found.</response>
    [HttpDelete("version/{versionId}/data/{dataId}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLauncherVersionData([BindRequired, FromRoute] ulong versionId, [BindRequired, FromRoute] ulong dataId)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }
            
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!_userManager.HasPermission(user, CustomPermissions.Launcher.DeleteVersion))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Permission denied.");

            LauncherVersionData? versionData =
                await _dbContext.FindLauncherVersionDataAsync(x => x.Id == dataId && x.VersionId == versionId);
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
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while deleting launcher version data.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    #endregion
}