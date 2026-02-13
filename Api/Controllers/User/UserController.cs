using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class UserController : CustomControllerBase
{
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public UserController(ILogger<UserController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpPost("skin/upload")]
    public async Task<IActionResult> UploadSkin(IFormFile file)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        if (file.Length > 1024 * 512) // 500 KB limit
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");
        
        if (!file.FileName.EndsWith(".png"))
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

        FileData? existingSkin = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
        if (existingSkin != null)
        {
            existingSkin.DeleteFile();
            await _dbContext.RemoveFileDataAsync(existingSkin, true);
        }

        await using var stream = file.OpenReadStream();
        using var sha256 = SHA256.Create();
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        string fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        stream.Position = 0;
        
        try 
        {
            // 4. Check Format and Dimensions using ImageSharp
            using var image = await Image.LoadAsync(stream);
            var info = image.Metadata.DecodedImageFormat;

            if (info?.Name != "PNG") 
                return BadRequest("Invalid image format (not a real PNG).");

            int width = image.Width;
            int height = image.Height;

            if (!((width == 64 && (height == 32 || height == 64)) || (width == 512 && (height == 256 || height == 512)))) 
                return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format. Expected dimensions: 64x32, 64x64, 512x256, or 512x512.");
                
            stream.Position = 0;
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to upload skin file: {fileHash}");
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
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
        return this.ReturnResponseCode(HttpStatusCode.OK, "Skin uploaded successfully");
    }
    
    [HttpDelete("skin")]
    public async Task<IActionResult> DeleteSkin()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        FileData? existingSkin = await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.SKIN);
        if (existingSkin == null)
            return this.ReturnResponseCode(HttpStatusCode.NotFound, "No skin found for the user");
        
        existingSkin.DeleteFile();
        await _dbContext.RemoveFileDataAsync(existingSkin, true);
        return this.ReturnResponseCode(HttpStatusCode.OK, "Skin deleted successfully");
    }

    [HttpPatch("cape/{id}")]
    public async Task<IActionResult> SelectCape(ulong id)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        UserCape? cape = _dbContext.FindUserCape(x => x.UserId == user.Id && x.CapeId == id);
        if (cape == null)
            return this.ReturnResponseCode(HttpStatusCode.NotFound, "Cape not found for the user");
        
        if (cape.IsSelected)
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Cape is already selected");
        
        UserCape? currentlySelectedCape = _dbContext.FindUserCape(x => x.UserId == user.Id && x.IsSelected);
        if (currentlySelectedCape != null)
        {
            currentlySelectedCape.IsSelected = false;
            await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
        }
        cape.IsSelected = true;
        await  _dbContext.UpdateUserCapeAsync(cape, true);
        return this.ReturnResponseCode(HttpStatusCode.OK, "Cape selected successfully");
    }

    [HttpDelete("cape")]
    public async Task<IActionResult> ClearSelectedCape()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

        // TODO: Add claim check

        UserCape? currentlySelectedCape = _dbContext.FindUserCape(x => x.UserId == user.Id && x.IsSelected);
        if (currentlySelectedCape == null)
            return this.ReturnResponseCode(HttpStatusCode.NotFound, "No cape is currently selected for the user");

        currentlySelectedCape.IsSelected = false;
        await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
        return this.ReturnResponseCode(HttpStatusCode.OK, "Selected cape cleared successfully");
    }

    #region Elevated Permissions

    [HttpPost("cape/upload")]
    public async Task<IActionResult> UploadCape(IFormFile file)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        if (file.Length > 1024 * 512) // 500 KB limit
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");
        
        if (!file.FileName.EndsWith(".png"))
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

        await using var stream = file.OpenReadStream();
        using var sha256 = SHA256.Create();
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        string fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        stream.Position = 0;
        
        FileData? existingCape = await _dbContext.FindFileDataAsync(x => x.Hash == fileHash && x.Type == EFileDataType.CAPE);
        if (existingCape != null)
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Cape with the same content already exists.");
        
        try 
        {
            // 4. Check Format and Dimensions using ImageSharp
            using var image = await Image.LoadAsync(stream);
            var info = image.Metadata.DecodedImageFormat;

            if (info?.Name != "PNG") 
                return BadRequest("Invalid image format (not a real PNG).");

            int width = image.Width;
            int height = image.Height;

            if (!((width == 64 && (height == 32 || height == 64)) || (width == 512 && (height == 256 || height == 512)))) 
                return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format. Expected dimensions: 64x32, 64x64, 512x256, or 512x512.");
                
            stream.Position = 0;
        }
        catch (Exception)
        {
            _logger.LogError($"Failed to upload cape file: {fileHash}");
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
        }

        FileData fd = await _dbContext.AddFileDataAsync(new FileData
        {
            Hash = fileHash,
            FileName = $"{Guid.NewGuid():N}.png",
            ContentType = "image/png",
            Type = EFileDataType.CAPE,
        }, true);
        fd.SaveFile(stream);
        Cape cape = await _dbContext.AddCapeAsync(new Cape
        {
            Name = file.FileName.Split('.')[0],
            FileId = fd.Id,
            IsPublic = true
        }, true);
        await _dbContext.AddUserCapeAsync(new UserCape
        {
            UserId = user.Id,
            CapeId = cape.Id,
            IsSelected = false,
            Reason = "Uploaded by user",
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        }, true);
        return this.ReturnResponseCode(HttpStatusCode.OK, "Cape uploaded successfully");
    }

    
    [HttpDelete("cape/{capeId}")]
    public async Task<IActionResult> DeleteCape(ulong capeId)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

        // TODO: Add claim check

        Cape? cape = _dbContext.FindCape(x => x.Id == capeId);
        if (cape == null)
            return this.ReturnResponseCode(HttpStatusCode.NotFound, "Cape not found");
        
        cape.FileData.DeleteFile();
        await _dbContext.RemoveFileDataAsync(cape.FileData);
        await _dbContext.RemoveCapeAsync(cape);

        var capes = _dbContext.GetUserCapes(x => x.CapeId == capeId);
        foreach (var userCape in capes)
          await _dbContext.RemoveUserCapeAsync(userCape);

        await _dbContext.SaveChangesAsync();
        return this.ReturnResponseCode(HttpStatusCode.OK, "Cape deleted successfully");
    }
    #endregion
}