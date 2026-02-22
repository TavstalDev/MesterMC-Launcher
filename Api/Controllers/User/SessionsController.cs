using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class SessionsController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public SessionsController(ILogger<SessionsController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        var userLogins = _dbContext.GetUserLogins(x => x.UserId == user.Id);
        return ReturnJson(userLogins);
    }

    [HttpDelete("sessions/{sessionId}")]
    public async Task<IActionResult> RevokeSession([BindRequired, FromRoute] ulong sessionId)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        var userLogin = _dbContext.FindUserLogin(x => x.Id == sessionId && x.UserId == user.Id);
        if (userLogin == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Session not found.");
        
        await _dbContext.RemoveUserLoginAsync(userLogin, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Session revoked successfully.");
    }

    [HttpDelete("sessions")]
    public async Task<IActionResult> RevokeAllSessions()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        await _dbContext.ClearUserLoginsAsync(user.Id, true);
        return ReturnResponseCode(HttpStatusCode.OK, "All sessions revoked successfully.");
    }
    
    #region Admin Endpoints
    [HttpGet("{userId}/sessions")]
    public async Task<IActionResult> GetSessionsAdmin([BindRequired, FromRoute] string userId)
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
        
        var userLogins = _dbContext.GetUserLogins(x => x.UserId == targetUser.Id);
        return ReturnJson(userLogins);
    }

    [HttpDelete("{userId}/sessions/{sessionId}")]
    public async Task<IActionResult> RevokeSessionAdmin([BindRequired, FromRoute] string userId, [BindRequired, FromRoute] ulong sessionId)
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
        
        var userLogin = _dbContext.FindUserLogin(x => x.Id == sessionId && x.UserId == targetUser.Id);
        if (userLogin == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Session not found.");
        
        await _dbContext.RemoveUserLoginAsync(userLogin, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Session revoked successfully.");
    }

    [HttpDelete("{userId}/sessions")]
    public async Task<IActionResult> RevokeAllSessionsAdmin([BindRequired, FromRoute] string userId)
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
        
        await _dbContext.ClearUserLoginsAsync(targetUser.Id, true);
        return ReturnResponseCode(HttpStatusCode.OK, "All sessions revoked successfully.");
    }
    #endregion
}