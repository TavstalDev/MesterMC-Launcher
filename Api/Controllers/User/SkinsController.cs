using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SixLabors.ImageSharp;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[ApiController]
[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class SkinsController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public SkinsController(ILogger<SkinsController > logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet("skin")]
    public async Task<IActionResult> GetSkin()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        FileData? skin = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
        if (skin == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
        
        if (!skin.Exists())
        {
            await _dbContext.RemoveFileDataAsync(skin, true);
            return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
        }
        
        return File(skin.GetFileStream(), skin.ContentType, skin.FileName);
    }
    
    [HttpPut("skin")]
    public async Task<IActionResult> UploadSkin([BindRequired] IFormFile file)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.Skins.Upload))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");
        
        if (file.Length > 1024 * 512) // 500 KB limit
            return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");
        
        if (!file.FileName.EndsWith(".png"))
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

        FileData? existingSkin = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
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

            if (!((width == 64 && (height == 32 || height == 64)) || (width == 512 && (height == 256 || height == 512)))) 
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format. Expected dimensions: 64x32, 64x64, 512x256, or 512x512.");
                
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
    
    [HttpDelete("skin")]
    public async Task<IActionResult> DeleteSkin()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.Skins.Delete))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");
        
        FileData? existingSkin = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
        if (existingSkin == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
        
        existingSkin.DeleteFile();
        await _dbContext.RemoveFileDataAsync(existingSkin, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Skin deleted successfully");
    }

    #region Admin Endpoints

    [HttpGet("{userId}/skin")]
    public async Task<IActionResult> GetSkinAdmin([BindRequired, FromRoute] string userId)
    {
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
        
        FileData? skin = await _dbContext.FindFileDataAsync(x => x.UserId == targetUser.Id && x.Type == EFileDataType.SKIN);
        if (skin == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
        
        if (!skin.Exists())
        {
            await _dbContext.RemoveFileDataAsync(skin, true);
            return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
        }
        
        return File(skin.GetFileStream(), skin.ContentType, skin.FileName);
    }
    
    [HttpPut("{userId}/skin")]
    public async Task<IActionResult> UploadSkinAdmin([BindRequired, FromRoute] string userId, [BindRequired] IFormFile file)
    {
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

        FileData? existingSkin = await _dbContext.FindFileDataAsync(x => x.UserId == targetUser.Id && x.Type == EFileDataType.SKIN);
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

            if (!((width == 64 && (height == 32 || height == 64)) || (width == 512 && (height == 256 || height == 512)))) 
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format. Expected dimensions: 64x32, 64x64, 512x256, or 512x512.");
                
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
    
    [HttpDelete("{userId}/skin")]
    public async Task<IActionResult> DeleteSkinAdmin([BindRequired, FromRoute] string userId)
    {
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
        
        FileData? existingSkin = await _dbContext.FindFileDataAsync(x => x.UserId == targetUser.Id && x.Type == EFileDataType.SKIN);
        if (existingSkin == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
        
        existingSkin.DeleteFile();
        await _dbContext.RemoveFileDataAsync(existingSkin, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Skin deleted successfully");
    }

    #endregion
}