using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Tavstal.MesterMC.Api.Controllers.Misc;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

/// <summary>
/// Controller for managing user capes.
/// </summary>
[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class UserCapesController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UserCapesController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="userManager">The custom user manager.</param>
    /// <param name="dbContext">The database context.</param>
    public UserCapesController(ILogger<CapesController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
    
    /// <summary>
    /// Selects a cape for the current user.
    /// </summary>
    /// <param name="capeId">The ID of the cape to select.</param>
    /// <returns>A success message or an appropriate HTTP status code.</returns>
    /// <response code="200">Cape selected successfully.</response>
    /// <response code="400">Cape is already selected.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to select a cape.</response>
    /// <response code="404">Cape not found for the user.</response>
    [HttpPatch("cape/{capeId}")]
    [TextResponse(StatusCodes.Status200OK),
     TextResponse(StatusCodes.Status400BadRequest),
     TextResponse(StatusCodes.Status401Unauthorized),
     TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SelectCape([BindRequired, FromRoute] ulong capeId)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Capes.Select))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            UserCape? cape = await _dbContext.FindUserCapeAsync(x => x.UserId == user.Id && x.CapeId == capeId);
            if (cape == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Cape not found for the user");

            if (cape.IsSelected)
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Cape is already selected");

            UserCape? currentlySelectedCape =
                await _dbContext.FindUserCapeAsync(x => x.UserId == user.Id && x.IsSelected);
            if (currentlySelectedCape != null)
            {
                currentlySelectedCape.IsSelected = false;
                await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
            }

            cape.IsSelected = true;
            await _dbContext.UpdateUserCapeAsync(cape, true);
            return ReturnResponseCode(HttpStatusCode.OK, "Cape selected successfully");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error while selecting cape.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Clears the currently selected cape for the current user.
    /// </summary>
    /// <returns>A success message or an appropriate HTTP status code.</returns>
    /// <response code="200">Selected cape cleared successfully.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to clear the selected cape.</response>
    /// <response code="404">No cape is currently selected for the user.</response>
    [HttpDelete("cape")]
    [TextResponse(StatusCodes.Status200OK),
     TextResponse(StatusCodes.Status401Unauthorized),
     TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearSelectedCape()
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!_userManager.HasPermission(user, CustomPermissions.Capes.Unselect))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            UserCape? currentlySelectedCape =
                await _dbContext.FindUserCapeAsync(x => x.UserId == user.Id && x.IsSelected);
            if (currentlySelectedCape == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No cape is currently selected for the user");

            currentlySelectedCape.IsSelected = false;
            await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
            return ReturnResponseCode(HttpStatusCode.OK, "Selected cape cleared successfully");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error while clearing selected cape.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    #region Admin Endpoints
    /// <summary>
    /// Selects a cape for another user (admin only).
    /// </summary>
    /// <param name="userId">The ID of the target user.</param>
    /// <param name="capeId">The ID of the cape to select.</param>
    /// <returns>A success message or an appropriate HTTP status code.</returns>
    /// <response code="200">Cape selected successfully.</response>
    /// <response code="400">Cape is already selected.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to select a cape for another user.</response>
    /// <response code="404">Target user or cape not found.</response>
    [HttpPatch("{userId}/cape/{capeId}")]
    [EnableRateLimiting(RateLimits.ADMIN)]
        [TextResponse(StatusCodes.Status200OK),
        TextResponse(StatusCodes.Status400BadRequest),
        TextResponse(StatusCodes.Status401Unauthorized),
        TextResponse(StatusCodes.Status403Forbidden),
        TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SelectCapeAdmin([BindRequired, FromRoute] string userId, [BindRequired, FromRoute] ulong capeId)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Capes.SelectOther))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

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

            UserCape? currentlySelectedCape =
                await _dbContext.FindUserCapeAsync(x => x.UserId == targetUser.Id && x.IsSelected);
            if (currentlySelectedCape != null)
            {
                currentlySelectedCape.IsSelected = false;
                await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
            }

            cape.IsSelected = true;
            await _dbContext.UpdateUserCapeAsync(cape, true);
            return ReturnResponseCode(HttpStatusCode.OK, "Cape selected successfully");
        }
        catch (Exception ex) 
        {
            Logger.LogCritical(ex, "Error while selecting cape for another user.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Clears the currently selected cape for another user (admin only).
    /// </summary>
    /// <param name="userId">The ID of the target user.</param>
    /// <returns>A success message or an appropriate HTTP status code.</returns>
    /// <response code="200">Selected cape cleared successfully.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="403">User does not have permission to clear the selected cape for another user.</response>
    /// <response code="404">Target user or selected cape not found.</response>
    [HttpDelete("{userId}/cape")]
    [EnableRateLimiting(RateLimits.ADMIN)]
        [TextResponse(StatusCodes.Status200OK),
            TextResponse(StatusCodes.Status401Unauthorized),
            TextResponse(StatusCodes.Status403Forbidden),
            TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClearSelectedCapeAdmin([BindRequired, FromRoute] string userId)
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

            if (!_userManager.HasPermission(user, CustomPermissions.Capes.UnselectOther))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

            CustomUser? targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Target user not found");

            if (!_userManager.HasHigherRoleThan(user, targetUser))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have permission to manage this user.");

            UserCape? currentlySelectedCape =
                await _dbContext.FindUserCapeAsync(x => x.UserId == targetUser.Id && x.IsSelected);
            if (currentlySelectedCape == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "No cape is currently selected for the user");

            currentlySelectedCape.IsSelected = false;
            await _dbContext.UpdateUserCapeAsync(currentlySelectedCape);
            return ReturnResponseCode(HttpStatusCode.OK, "Selected cape cleared successfully");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Error while clearing selected cape for another user.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    #endregion
}