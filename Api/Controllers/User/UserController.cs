using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
public class UserController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public UserController(IConfiguration configuration, ILogger<UserController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpPost("skin/upload")]
    public async Task<IActionResult> UploadSkin(IFormFile file)
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Authorization header is missing");
        
        CustomUser? user = await _userManager.GetUserByAuthenticationStringAsync(Request.Headers["Authorization"]);
        if (user == null)
            return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Failed to authenticate user with provided Authorization header");
        
        if (file.Length > 1024 * 512) // 500 KB limit
            return this.ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 1 MB limit.");
        
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
            return BadRequest("Could not process image.");
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
        return Ok();
    }
    
    [HttpDelete("skin")]
    public async Task<IActionResult> DeleteSkin()
    {
        // TODO
        return Ok();
    }

    [HttpPatch("cape/{id}")]
    public async Task<IActionResult> SelectCape(string id)
    {
        // TODO
        return Ok();
    }
}