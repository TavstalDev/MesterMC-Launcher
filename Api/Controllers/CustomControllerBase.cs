using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers;

/// <summary>
/// Base controller class that provides common functionality for derived controllers.
/// </summary>
public abstract class CustomControllerBase : Controller
{
    /// <summary>
    /// Gets the user ID of the currently authenticated user.
    /// </summary>
    protected string? UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Retrieves the current user asynchronously using the provided user manager.
    /// </summary>
    /// <param name="manager">The user manager used to find the user by their ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current user.</returns>
    protected async Task<CustomUser?> GetCurrentUserAsync(CustomUserManager manager)
    {
        if (string.IsNullOrEmpty(UserId))
            return null;
        return await manager.FindByIdAsync(UserId);
    }
}