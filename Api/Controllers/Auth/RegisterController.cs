using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Bodies.Auth;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;
using Tavstal.MesterMC.Api.Utils.Helpers;

namespace Tavstal.MesterMC.Api.Controllers.Auth;

[ApiController]
[Route("/register")]
[Tags("Authentication: Registration")]
public class RegisterController : Controller
{
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly EmailService _emailService;
    private readonly Settings _settings;
    
    public RegisterController(ILogger<RegisterController> logger, CustomDbContext dbContext, CustomUserManager userManager, EmailService emailService, Settings settings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _userManager = userManager;
        _emailService = emailService;
        _settings = settings;
    }
    
    [HttpPost("")]
    [TextResponse(StatusCodes.Status201Created), TextResponse(StatusCodes.Status400BadRequest), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status409Conflict), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestBody request)
    {
        try
        {
            // Checks if the request is valid
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if (await _userManager.IsCompromisedPassword(request.Password)) 
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Password is compromised.");
            
            if (!request.EmailAddress.IsValidEmail())
                return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid email address.");
                
            var normalizedEmail = request.EmailAddress.Normalize();
            var normalizedUsername = request.Username.Normalize();
            CustomUser? user = await _dbContext.FindUserAsync(x => x.NormalizedEmail == normalizedEmail || x.NormalizedUserName == normalizedUsername);
            if (user != null)
                return this.ReturnResponseCode(HttpStatusCode.Conflict, "User already exists.");

            // TODO: Handle avatar upload and saving
            /*string? avatarPath = null;
            if (request.Avatar is { Length: > 0 })
            {
                avatarPath = $"~/wwwroot/avatars/{normalizedUsername}.png";
                using var memoryStream = new MemoryStream();
                await request.Avatar.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                if (!await IOHelper.SaveFileAsync(avatarPath, fileBytes, ["image/png"]))
                    avatarPath = null;
            }*/
            
            user = await _dbContext.AddUserAsync(
                new CustomUser(request.Username, normalizedUsername, request.EmailAddress, normalizedEmail, 
                    StringChiper.GetEncryptedSha256Hash(request.Password, _settings.EncryptionKey), ESkinType.WIDE,
                    null, string.Empty, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
                true);
            
            // Add the user to the default role
            var defaultRole = _dbContext.FindRole(x => x.NormalizedName == "DEFAULT");
            if (defaultRole != null)
            {
                await _dbContext.AddUserRoleAsync(new CustomUserRole { UserId = user.Id, RoleId = defaultRole.Id, });
            }
            // Add customer role claim to the user
            await _dbContext.AddUserClaimAsync(new CustomUserClaim { UserId = user.Id, ClaimType = ClaimTypes.Role, ClaimValue = "default" });
            // Save changes
            await _dbContext.SaveChangesAsync();
            
            // Send registration confirmation email
            try
            {
                await SendConfirmEmail(user);
            }
            catch (Exception eex)
            {
                _logger.LogCritical("Failed to send confirmation email: {Message}", eex);
            }

            return this.ReturnResponseCode(HttpStatusCode.Created, "User registered successfully");
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Error during registration: {Message}", ex);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred");
        }
    }

    
    [HttpPatch("confirm")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest),
     TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmRegistration([BindRequired, FromBody] ConfirmRegisterRequestBody request)
    {
        try
        {
            // Find the user by ID
            CustomUser? user = await _dbContext.FindUserAsync(x => x.Id == request.UserId);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "User does not exist.");

            // Check if the user's email is already confirmed
            if (user.EmailConfirmed)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "The user is already confirmed.");

            // Validate the confirmation token
            var claim = _dbContext.FindUserClaim(x =>
                x.UserId == request.UserId && x.ClaimType == CustomClaimTypes.EmailConfirmationToken &&
                x.ClaimValue == request.ConfirmationToken);
            if (claim == null)
                return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid confirmation token");

            // Remove the confirmation token claim and update the user's email confirmation status
            await _dbContext.RemoveUserClaimAsync(claim);
            user.EmailConfirmed = true;
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();

            // Send a confirmation email to the user
            await _emailService.SendEmailAsync(user.Email, "Account Confirmation",
                "<h1>Your account has been confirmed</h1><p>Thank you for confirming your account. You can now log in.</p>");

            return this.ReturnResponseCode(HttpStatusCode.OK, "User confirmed successfully");
        }
        catch (Exception ex)
        {
            // Log critical errors and return an internal server error response
            _logger.LogCritical("Error during email confirmation: {Message}", ex);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred");
        }
    }

    
    private async Task SendConfirmEmail(CustomUser user)
    {
        if (string.IsNullOrEmpty(user.Email))
            return;
        
        var claim = _dbContext.FindUserClaim(x => x.UserId == user.Id && x.ClaimType == CustomClaimTypes.EmailConfirmationToken);
        string token;
        if (claim == null || string.IsNullOrEmpty(claim.ClaimValue))
        {
            token = TokenHelper.GenerateAccountConfirmationToken();
            await _dbContext.AddUserClaimAsync(
                new CustomUserClaim
                {
                    UserId = user.Id, ClaimType = CustomClaimTypes.EmailConfirmationToken,
                    ClaimValue = token
                }, true);
        }
        else
            token = claim.ClaimValue;
        
        await _emailService.SendEmailAsync(user.Email, "Registration Confirmation",
            $"<h1>Confirm your email</h1><p>Click <a href=\"{_settings.WebsiteUrl}/register/confirm?userId={user.Id}&token={Uri.EscapeDataString(token)}\">here</a> to confirm your email.</p>");
    }
}