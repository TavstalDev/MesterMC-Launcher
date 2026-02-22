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
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    // TODO: Add following endpoints:
    // - GET /user/{id}: Get public information about a user by their ID (e.g., username, avatar URL, registration date).
    // - GET /user/{id}/avatar: Get the avatar image for a user by their ID.
    // - GET /user/{id}/status: Get the online status of a user by their ID (e.g., online, offline, last seen).
    // - GET /user/{id}/stats: Get public statistics about a user by their ID (e.g., number of logins, last login date).
    // - GET /user/{id}/badges: Get a list of badges or achievements for a user by their ID. 
    
    public PublicUserController(ILogger<PublicUserController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
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