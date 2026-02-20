using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SixLabors.ImageSharp;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class AvatarController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public AvatarController(ILogger<AvatarController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
    
    [HttpGet("avatar")]
    public async Task<IActionResult> GetAvatar()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        FileData? existingAvatar = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.PROFILE_PICTURE);
        if (existingAvatar == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No avatar found to delete.");
        
        string etag = $"\"{existingAvatar.Hash}\"";
        if (Request.Headers.TryGetValue("If-None-Match", out var incomingEtag) &&
            incomingEtag == etag)
        {
            return ReturnResponseCode(HttpStatusCode.NotModified);
        }

        Response.Headers.CacheControl =
            "public,max-age=3600,immutable";
        return File(existingAvatar.GetFileStream(), existingAvatar.ContentType, existingAvatar.FileName, enableRangeProcessing: true);
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar([BindRequired] IFormFile file)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        if (file.Length > 1024 * 512) // 500 KB limit
            return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");
        
        if (!file.FileName.EndsWith(".png"))
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

        await using var stream = file.OpenReadStream();
        using var sha256 = SHA256.Create();
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        string fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        stream.Position = 0;
        
        try 
        {
            // Check Format and Dimensions using ImageSharp
            using var image = await Image.LoadAsync(stream);
            var info = image.Metadata.DecodedImageFormat;

            if (info?.Name != "PNG") 
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format (not a real PNG).");

            stream.Position = 0;
        }
        catch (Exception)
        {
            Logger.LogError($"Failed to upload avatar file: {fileHash}");
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
        }
        
        FileData? existingAvatar = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.PROFILE_PICTURE);
        if (existingAvatar != null)
        {
            existingAvatar.DeleteFile();
            await _dbContext.RemoveFileDataAsync(existingAvatar, true);
        }

        FileData fd = await _dbContext.AddFileDataAsync(new FileData
        {
            Hash = fileHash,
            FileName = $"{Guid.NewGuid():N}.png",
            ContentType = "image/png",
            Type = EFileDataType.PROFILE_PICTURE,
            UserId = user.Id
        }, true);
        fd.SaveFile(stream);
        return ReturnResponseCode(HttpStatusCode.OK, "Avatar uploaded successfully.");
    }

    [HttpDelete("avatar")]
    public async Task<IActionResult> DeleteAvatar()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        FileData? existingAvatar = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.PROFILE_PICTURE);
        if (existingAvatar == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No avatar found to delete.");
        
        existingAvatar.DeleteFile();
        await _dbContext.RemoveFileDataAsync(existingAvatar, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Avatar deleted successfully.");
    }

    #region Admin Endpoints
    [HttpPost("{userId}/avatar")]
    public async Task<IActionResult> UploadAvatarAdmin([BindRequired, FromQuery] string userId, [BindRequired] IFormFile file)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        CustomUser? targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Target user not found");
        
        if (!_userManager.HasHigherRoleThan(user, targetUser))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have permission to manage this user.");
        
        if (file.Length > 1024 * 512) // 500 KB limit
            return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");
        
        if (!file.FileName.EndsWith(".png"))
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

        await using var stream = file.OpenReadStream();
        using var sha256 = SHA256.Create();
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        string fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        stream.Position = 0;
        
        try 
        {
            // Check Format and Dimensions using ImageSharp
            using var image = await Image.LoadAsync(stream);
            var info = image.Metadata.DecodedImageFormat;

            if (info?.Name != "PNG") 
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format (not a real PNG).");

            stream.Position = 0;
        }
        catch (Exception)
        {
            Logger.LogError($"Failed to upload avatar file: {fileHash}");
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
        }
        
        FileData? existingAvatar = await _dbContext.FindFileDataAsync(x => x.UserId == targetUser.Id && x.Type == EFileDataType.PROFILE_PICTURE);
        if (existingAvatar != null)
        {
            existingAvatar.DeleteFile();
            await _dbContext.RemoveFileDataAsync(existingAvatar, true);
        }

        FileData fd = await _dbContext.AddFileDataAsync(new FileData
        {
            Hash = fileHash,
            FileName = $"{Guid.NewGuid():N}.png",
            ContentType = "image/png",
            Type = EFileDataType.PROFILE_PICTURE,
            UserId = targetUser.Id
        }, true);
        fd.SaveFile(stream);
        return ReturnResponseCode(HttpStatusCode.OK, "Avatar uploaded successfully.");
    }

    [HttpDelete("{userId}/avatar")]
    public async Task<IActionResult> DeleteAvatarAdmin([BindRequired, FromQuery] string userId)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        CustomUser? targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Target user not found");
        
        if (!_userManager.HasHigherRoleThan(user, targetUser))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have permission to manage this user.");
        
        FileData? existingAvatar = await _dbContext.FindFileDataAsync(x => x.UserId == targetUser.Id && x.Type == EFileDataType.PROFILE_PICTURE);
        if (existingAvatar == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No avatar found to delete.");
        
        existingAvatar.DeleteFile();
        await _dbContext.RemoveFileDataAsync(existingAvatar, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Avatar deleted successfully.");
    }
    #endregion
}