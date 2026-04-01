using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

/// <summary>
/// Controller for managing public user-related operations.
/// </summary>
[Route("/user")]
public class PublicUserController : CustomControllerBase
{
    private readonly Settings _settings;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="PublicUserController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="userManager">The custom user manager.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="settings">The application settings.</param>
    public PublicUserController(ILogger<PublicUserController> logger, CustomUserManager userManager, CustomDbContext dbContext, Settings settings) : base(logger, settings)
    {
        _settings = settings;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves information about a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve information for.</param>
    /// <returns>A JSON object containing user information or an appropriate HTTP status code.</returns>
    /// <response code="200">User information retrieved successfully.</response>
    /// <response code="404">User not found.</response>
    [HttpGet("{userId}")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserInfo([BindRequired, FromRoute] string userId)
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

            CustomUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

            string avatarUrl = string.Empty;
            if (user.Avatar != null && !string.IsNullOrEmpty(_settings.ApiUrl))
                avatarUrl = user.Avatar.GetUrl(_settings.ApiUrl);
            
            return ReturnJson(new
            {
                user.Id,
                AvatarUrl = avatarUrl,
                user.DiscordId,
                user.UserName,
                user.CreateDate,
                user.LastUpdate,
                user.LockoutEnabled,
                user.LockoutEnd,
                user.LockoutReason
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while retrieving user information.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Retrieves the avatar of a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose avatar is to be retrieved.</param>
    /// <returns>The avatar file or an appropriate HTTP status code.</returns>
    /// <response code="200">Avatar retrieved successfully.</response>
    /// <response code="304">Avatar not modified (ETag matches).</response>
    /// <response code="404">User or avatar not found.</response>
    [HttpGet("{userId}/avatar")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status304NotModified), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvatar([BindRequired, FromRoute] string userId)
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

            CustomUser? user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

            FileData? existingAvatar =
                await _dbContext.FindFileDataAsync(x => x.UserId == user.Id && x.Type == EFileDataType.PROFILE_PICTURE);
            if (existingAvatar == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No avatar found.");

            string etag = $"\"{existingAvatar.Hash}\"";
            if (Request.Headers.TryGetValue("If-None-Match", out var incomingEtag) &&
                incomingEtag == etag)
            {
                return ReturnResponseCode(HttpStatusCode.NotModified);
            }

            Response.Headers.CacheControl =
                "public,max-age=3600,immutable";
            return File(existingAvatar.GetFileStream(), existingAvatar.ContentType, existingAvatar.FileName,
                enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while retrieving the user's avatar.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
}