using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Controllers.Misc;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class UserCapesController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    public UserCapesController(ILogger<CapesController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
    
    [HttpPatch("cape/{capeId}")]
    public async Task<IActionResult> SelectCape([BindRequired, FromRoute] ulong capeId)
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        // TODO: Add claim check
        
        UserCape? cape = await _dbContext.FindUserCapeAsync(x => x.UserId == user.Id && x.CapeId == capeId);
        if (cape == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Cape not found for the user");
        
        if (cape.IsSelected)
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Cape is already selected");
        
        UserCape? currentlySelectedCape = await _dbContext.FindUserCapeAsync(x => x.UserId == user.Id && x.IsSelected);
        if (currentlySelectedCape != null)
        {
            currentlySelectedCape.IsSelected = false;
            await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
        }
        cape.IsSelected = true;
        await  _dbContext.UpdateUserCapeAsync(cape, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Cape selected successfully");
    }

    [HttpDelete("cape")]
    public async Task<IActionResult> ClearSelectedCape()
    {
        CustomUser? user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

        // TODO: Add claim check

        UserCape? currentlySelectedCape = await _dbContext.FindUserCapeAsync(x => x.UserId == user.Id && x.IsSelected);
        if (currentlySelectedCape == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No cape is currently selected for the user");

        currentlySelectedCape.IsSelected = false;
        await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
        return ReturnResponseCode(HttpStatusCode.OK, "Selected cape cleared successfully");
    }

    #region Admin Endpoints
    [HttpPatch("{userId}/cape/{capeId}")]
    public async Task<IActionResult> SelectCapeAdmin([BindRequired, FromRoute] string userId, [BindRequired, FromRoute] ulong capeId)
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
        
        UserCape? cape = await _dbContext.FindUserCapeAsync(x => x.UserId == targetUser.Id && x.CapeId == capeId);
        if (cape == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "Cape not found for the user");
        
        if (cape.IsSelected)
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Cape is already selected");
        
        UserCape? currentlySelectedCape = await _dbContext.FindUserCapeAsync(x => x.UserId == targetUser.Id && x.IsSelected);
        if (currentlySelectedCape != null)
        {
            currentlySelectedCape.IsSelected = false;
            await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
        }
        cape.IsSelected = true;
        await  _dbContext.UpdateUserCapeAsync(cape, true);
        return ReturnResponseCode(HttpStatusCode.OK, "Cape selected successfully");
    }

    [HttpDelete("{userId}/cape")]
    public async Task<IActionResult> ClearSelectedCapeAdmin([BindRequired, FromRoute] string userId)
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

        UserCape? currentlySelectedCape = await _dbContext.FindUserCapeAsync(x => x.UserId == targetUser.Id && x.IsSelected);
        if (currentlySelectedCape == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "No cape is currently selected for the user");

        currentlySelectedCape.IsSelected = false;
        await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
        return ReturnResponseCode(HttpStatusCode.OK, "Selected cape cleared successfully");
    }
    #endregion
}