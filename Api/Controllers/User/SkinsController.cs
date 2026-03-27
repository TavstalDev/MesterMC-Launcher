using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using SixLabors.ImageSharp;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

/// <summary>
/// Controller for managing user skins.
/// </summary>
[ApiController]
[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class SkinsController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="SkinsController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="userManager">The custom user manager.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="settings">Application settings.</param>
    public SkinsController(ILogger<SkinsController > logger, CustomUserManager userManager, CustomDbContext dbContext, Settings settings) : base(logger, settings)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves the current user's skin.
    /// </summary>
    /// <returns>The skin file or an appropriate HTTP status code.</returns>
    /// <response code="200">Skin retrieved successfully.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="404">No skin found for the user.</response>
    [HttpGet("skin")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSkin()
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!_userManager.HasPermission(user, CustomPermissions.Skins.View))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            FileData? skin =
                await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
            if (skin == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");

            if (!skin.Exists())
            {
                await _dbContext.RemoveFileDataAsync(skin, true);
                return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
            }

            return File(skin.GetFileStream(), skin.ContentType, skin.FileName);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "An error occurred while retrieving the skin.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Uploads a new skin for the current user.
    /// </summary>
    /// <param name="file">The skin file to upload. Must be a PNG file and meet size and dimension requirements.</param>
    /// <returns>A success message or an appropriate HTTP status code.</returns>
    /// <response code="200">Skin uploaded successfully.</response>
    /// <response code="400">Invalid file format, size, or dimensions.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to upload a skin.</response>
    [HttpPut("skin")]
    [EnableRateLimiting(RateLimits.UPLOAD)]
    [Consumes("multipart/form-data")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), TextResponse(StatusCodes.Status401Unauthorized), 
     TextResponse(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadSkin([BindRequired] IFormFile file)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Skins.Upload))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            if (file.Length > 1024 * 512) // 500 KB limit
                return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");

            if (!file.FileName.EndsWith(".png"))
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

            FileData? existingSkin =
                await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
            if (existingSkin != null)
            {
                existingSkin.DeleteFile();
                await _dbContext.RemoveFileDataAsync(existingSkin, true);
            }

            await using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            stream.Position = 0;

            try
            {
                // Check Format and Dimensions using ImageSharp
                using var image = await Image.LoadAsync(stream);
                var info = image.Metadata.DecodedImageFormat;

                if (info?.Name != "PNG")
                    return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format (not a real PNG).");

                int width = image.Width;
                int height = image.Height;

                if (!((width == 64 && (height == 32 || height == 64)) ||
                      (width == 512 && (height == 256 || height == 512))))
                    return ReturnResponseCode(HttpStatusCode.BadRequest,
                        "Invalid image format. Expected dimensions: 64x32, 64x64, 512x256, or 512x512.");

                stream.Position = 0;
            }
            catch (Exception)
            {
                Logger.LogError($"Failed to upload skin file: {fileHash}");
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
            }

            FileData fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = $"{Guid.NewGuid():N}.png",
                ContentType = "image/png",
                UserId = user.Id,
                Type = EFileDataType.SKIN,
            }, true);
            fd.SaveFile(stream);
            return ReturnResponseCode(HttpStatusCode.OK, "Skin uploaded successfully");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "An error occurred while uploading the skin.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Deletes the current user's skin.
    /// </summary>
    /// <returns>A success message or an appropriate HTTP status code.</returns>
    /// <response code="200">Skin deleted successfully.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to delete the skin.</response>
    /// <response code="404">No skin found for the user.</response>
    [HttpDelete("skin")]
    [EnableRateLimiting(RateLimits.WRITE)]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSkin()
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!_userManager.HasPermission(user, CustomPermissions.Skins.Delete))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            FileData? existingSkin =
                await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
            if (existingSkin == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");

            existingSkin.DeleteFile();
            await _dbContext.RemoveFileDataAsync(existingSkin, true);
            return ReturnResponseCode(HttpStatusCode.OK, "Skin deleted successfully");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "An error occurred while deleting the skin.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    #region Admin Endpoints

    /// <summary>
    /// Retrieves the skin of a specific user (admin only).
    /// </summary>
    /// <param name="userId">The ID of the target user.</param>
    /// <returns>The skin file or an appropriate HTTP status code.</returns>
    /// <response code="200">Skin retrieved successfully.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to view the skin.</response>
    /// <response code="404">No skin found for the user.</response>
    [HttpGet("{userId}/skin")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSkinAdmin([BindRequired, FromRoute] string userId)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Skins.ViewOther))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            CustomUser? targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Target user not found");

            if (!_userManager.HasHigherRoleThan(user, targetUser))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have permission to manage this user.");

            FileData? skin =
                await _dbContext.FindFileDataAsync(x => x.UserId == targetUser.Id && x.Type == EFileDataType.SKIN);
            if (skin == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");

            if (!skin.Exists())
            {
                await _dbContext.RemoveFileDataAsync(skin, true);
                return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
            }

            return File(skin.GetFileStream(), skin.ContentType, skin.FileName);
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "An error occurred while retrieving the skin.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Uploads a new skin for a specific user (admin only).
    /// </summary>
    /// <param name="userId">The ID of the target user.</param>
    /// <param name="file">The skin file to upload. Must be a PNG file and meet size and dimension requirements.</param>
    /// <returns>A success message or an appropriate HTTP status code.</returns>
    /// <response code="200">Skin uploaded successfully.</response>
    /// <response code="400">Invalid file format, size, or dimensions.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to upload the skin.</response>
    /// <response code="404">Target user not found.</response>
    [HttpPut("{userId}/skin")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [Consumes("multipart/form-data")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), TextResponse(StatusCodes.Status401Unauthorized), 
     TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadSkinAdmin([BindRequired, FromRoute] string userId, [BindRequired] IFormFile file)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Skins.UploadOther))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            CustomUser? targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Target user not found");

            if (!_userManager.HasHigherRoleThan(user, targetUser))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have permission to manage this user.");

            if (file.Length > 1024 * 512) // 500 KB limit
                return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");

            if (!file.FileName.EndsWith(".png"))
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

            FileData? existingSkin =
                await _dbContext.FindFileDataAsync(x => x.UserId == targetUser.Id && x.Type == EFileDataType.SKIN);
            if (existingSkin != null)
            {
                existingSkin.DeleteFile();
                await _dbContext.RemoveFileDataAsync(existingSkin, true);
            }

            await using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            stream.Position = 0;

            try
            {
                // Check Format and Dimensions using ImageSharp
                using var image = await Image.LoadAsync(stream);
                var info = image.Metadata.DecodedImageFormat;

                if (info?.Name != "PNG")
                    return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format (not a real PNG).");

                int width = image.Width;
                int height = image.Height;

                if (!((width == 64 && (height == 32 || height == 64)) ||
                      (width == 512 && (height == 256 || height == 512))))
                    return ReturnResponseCode(HttpStatusCode.BadRequest,
                        "Invalid image format. Expected dimensions: 64x32, 64x64, 512x256, or 512x512.");

                stream.Position = 0;
            }
            catch (Exception)
            {
                Logger.LogError($"Failed to upload skin file: {fileHash}");
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
            }

            FileData fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = $"{Guid.NewGuid():N}.png",
                ContentType = "image/png",
                UserId = targetUser.Id,
                Type = EFileDataType.SKIN,
            }, true);
            fd.SaveFile(stream);
            return ReturnResponseCode(HttpStatusCode.OK, "Skin uploaded successfully");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "An error occurred while uploading the skin.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Deletes the skin of a specific user (admin only).
    /// </summary>
    /// <param name="userId">The ID of the target user.</param>
    /// <returns>A success message or an appropriate HTTP status code.</returns>
    /// <response code="200">Skin deleted successfully.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to delete the skin.</response>
    /// <response code="404">No skin found for the user.</response>
    [HttpDelete("{userId}/skin")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSkinAdmin([BindRequired, FromRoute] string userId)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Skins.DeleteOther))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            CustomUser? targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Target user not found");

            if (!_userManager.HasHigherRoleThan(user, targetUser))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have permission to manage this user.");

            FileData? existingSkin =
                await _dbContext.FindFileDataAsync(x => x.UserId == targetUser.Id && x.Type == EFileDataType.SKIN);
            if (existingSkin == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");

            existingSkin.DeleteFile();
            await _dbContext.RemoveFileDataAsync(existingSkin, true);
            return ReturnResponseCode(HttpStatusCode.OK, "Skin deleted successfully");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "An error occurred while deleting the skin.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    #endregion
}