using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
public class PublicUserController : CustomControllerBase
{
    private readonly Settings _settings;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public PublicUserController(ILogger<PublicUserController> logger, CustomUserManager userManager, CustomDbContext dbContext, Settings settings) : base(logger)
    {
        _settings = settings;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserInfo([BindRequired, FromRoute] string userId)
    {
        CustomUser? user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

        return ReturnJson(new
        {
            user.Id,
            AvatarUrl = user.Avatar?.GetUrl(_settings.ApiUrl),
            user.DiscordId,
            user.UserName,
            user.CreateDate,
            user.LastUpdate,
            user.LockoutEnabled,
            user.LockoutEnd,
            user.LockoutReason
        });
    }

    [HttpGet("{userId}/avatar")]
    public async Task<IActionResult> GetAvatar([BindRequired, FromRoute] string userId)
    {
        CustomUser? user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");
        
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
}