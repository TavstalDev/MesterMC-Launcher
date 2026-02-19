using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.User;

[Route("/user")]
[Authorize(AuthenticationSchemes = "Bearer,Basic")]
public class NotificationsController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    
    // TODO: Add following endpoints:
    // - GET /user/notifications: Get a list of notifications for the current user.
    // - POST /user/notifications/{notificationId}/read: Mark a specific notification as read by its ID.
    // - DELETE /user/notifications/{notificationId}: Delete a specific notification by its ID.
    // - DELETE /user/notifications: Delete all notifications for the current user.
    // - GET /user/{id}/notifications: Get a list of notifications for a specific user by their ID (admin only).
    // - POST /user/{id}/notifications/{notificationId}/read: Mark a specific notification as read for a specific user by their ID (admin only).
    // - DELETE /user/{id}/notifications/{notificationId}: Delete a specific notification for a specific user by their ID (admin only).
    // - DELETE /user/{id}/notifications: Delete all notifications for a specific user by their ID (admin only).
    
    public NotificationsController(ILogger<NotificationsController> logger, CustomUserManager userManager, CustomDbContext dbContext) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
    }
}