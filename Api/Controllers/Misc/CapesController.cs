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

namespace Tavstal.MesterMC.Api.Controllers.Misc;

/// <summary>
/// Controller for managing capes, including uploading and deleting capes.
/// </summary>
[Route("/capes")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class CapesController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CapesController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="userManager">Custom user manager for user operations.</param>
    /// <param name="dbContext">Database context for accessing cape data.</param>
    /// <param name="settings">Application settings.</param>
    public CapesController(ILogger<CapesController> logger, CustomUserManager userManager, CustomDbContext dbContext, Settings settings) : base(logger, settings)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Uploads a new cape for the authenticated user.
    /// </summary>
    /// <param name="file">The cape file to upload.</param>
    /// <response code="200">Cape uploaded successfully.</response>
    /// <response code="400">Invalid file format, size, or duplicate content.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    [HttpPost]
    [EnableRateLimiting(RateLimits.UPLOAD)]
    [Consumes("multipart/form-data")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), 
     TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadCape([BindRequired, FormFile(500, EFileSizeUnit.Kilobytes)] IFormFile file)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Capes.Create))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Permission denied.");

            if (file.Length > 1024 * 512) // 500 KB limit
                return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");

            if (!file.FileName.EndsWith(".png"))
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

            await using var stream = file.OpenReadStream();
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            stream.Position = 0;

            FileData? existingCape =
                await _dbContext.FindFileDataAsync(x => x.Hash == fileHash && x.Type == EFileDataType.CAPE);
            if (existingCape != null)
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Cape with the same content already exists.");

            try
            {
                // 4. Check Format and Dimensions using ImageSharp
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
                Logger.LogError($"Failed to upload cape file: {fileHash}");
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
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
            return ReturnResponseCode(HttpStatusCode.OK, "Cape uploaded successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error uploading cape");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    
    /// <summary>
    /// Deletes a cape by its ID.
    /// </summary>
    /// <param name="capeId">The ID of the cape to delete.</param>
    /// <response code="200">Cape deleted successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="404">Cape not found.</response>
    [HttpDelete("{capeId}")]
    [EnableRateLimiting(RateLimits.ADMIN)]
        [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), 
        TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden),
        TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCape([BindRequired, FromRoute] ulong capeId)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Capes.Delete))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Permission denied.");

            Cape? cape = await _dbContext.FindCapeAsync(x => x.Id == capeId);
            if (cape == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Cape not found");

            cape.FileData.DeleteFile();
            await _dbContext.RemoveFileDataAsync(cape.FileData);
            await _dbContext.RemoveCapeAsync(cape);

            var capes = await _dbContext.GetUserCapesAsync(x => x.CapeId == capeId);
            foreach (var userCape in capes)
                await _dbContext.RemoveUserCapeAsync(userCape);

            await _dbContext.SaveChangesAsync();
            return ReturnResponseCode(HttpStatusCode.OK, "Cape deleted successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Failed to delete cape with ID {capeId}");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
}