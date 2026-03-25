using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SixLabors.ImageSharp;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Bodies.Auth;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;
using Tavstal.MesterMC.Api.Utils.Helpers;

namespace Tavstal.MesterMC.Api.Controllers.Auth;

/// <summary>
/// Controller for handling user registration and email confirmation.
/// </summary>
[ApiController]
[Route("/register")]
[Tags("Authentication: Registration")]
public class RegisterController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly Settings _settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="dbContext">Database context for accessing user data.</param>
    /// <param name="userManager">Custom user manager for user operations.</param>
    /// <param name="emailService">Service for sending emails.</param>
    /// <param name="settings">Application settings.</param>
    public RegisterController(ILogger<RegisterController> logger, CustomDbContext dbContext, CustomUserManager userManager, IEmailService emailService, Settings settings) : base(logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _emailService = emailService;
        _settings = settings;
    }

    /// <summary>
    /// Registers a new user using a multipart/form-data request.
    /// </summary>
    /// <param name="request">The registration request body containing user details and an optional avatar file.</param>
    /// <response code="201">User registered successfully.</response>
    /// <response code="400">Bad request. Invalid input data.</response>
    /// <response code="403">Forbidden. Password is compromised.</response>
    /// <response code="409">Conflict. User already exists.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPost("")]
    [EnableRateLimiting(RateLimits.AUTH_REGISTER)]
    [Consumes("multipart/form-data")]
    [TextResponse(StatusCodes.Status201Created), TextResponse(StatusCodes.Status400BadRequest),
     TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status409Conflict), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterForm([FromForm] RegisterRequestBody request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest, string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }
            
            if (await _userManager.IsCompromisedPasswordAsync(request.Password)) 
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Password is compromised.");
            
            if (!request.EmailAddress.IsValidEmail())
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid email address.");
                
            var normalizedEmail = request.EmailAddress.Normalize();
            var normalizedUsername = request.Username.Normalize();
            CustomUser? user = await _dbContext.FindUserAsync(x => x.NormalizedEmail == normalizedEmail || x.NormalizedUserName == normalizedUsername);
            if (user != null)
                return ReturnResponseCode(HttpStatusCode.Conflict, "User already exists.");
            
            FileData? avatarData = null;
            if (request.Avatar is { Length: > 0 })
            {
                await using var stream = request.Avatar.OpenReadStream();
                using var sha256 = SHA256.Create();
                byte[] hashBytes = await sha256.ComputeHashAsync(stream);
                string fileHash = Convert.ToHexStringLower(hashBytes);
                stream.Position = 0;
                
                try 
                {
                    // Check Format and Dimensions using ImageSharp
                    using var image = await Image.LoadAsync(stream);
                    var info = image.Metadata.DecodedImageFormat;

                    if (info?.Name != "PNG") 
                        return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format (not a real PNG).");

                    stream.Position = 0;
                }
                catch (Exception)
                {
                    Logger.LogError($"Failed to upload avatar file: {fileHash}");
                    return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
                }
                
                avatarData = await _dbContext.AddFileDataAsync(new FileData
                {
                    Hash = fileHash,
                    FileName = $"{Guid.NewGuid():N}.png",
                    ContentType = "image/png",
                    Type = EFileDataType.PROFILE_PICTURE
                }, true);
                avatarData.SaveFile(stream);
            }
            
            user = await _dbContext.AddUserAsync(
                new CustomUser(request.Username, normalizedUsername, request.EmailAddress, normalizedEmail, 
                    StringChiper.GetEncryptedSha256Hash(request.Password, _settings.EncryptionKey), ESkinType.WIDE,
                    null, string.Empty, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow),
                true);
            if (avatarData != null)
            {
                avatarData.UserId = user.Id;
                await _dbContext.UpdateFileDataAsync(avatarData);
            }
            
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
                Logger.LogCritical("Failed to send confirmation email: {Message}", eex);
            }

            return ReturnResponseCode(HttpStatusCode.Created, "User registered successfully");
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Error during registration: {Message}", ex);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred");
        }
    }
    
    /// <summary>
    /// Confirms a user's registration using a confirmation token.
    /// </summary>
    /// <param name="request">The confirmation request body containing user ID and token.</param>
    /// <response code="200">User confirmed successfully.</response>
    /// <response code="400">Bad request. Invalid confirmation token.</response>
    /// <response code="403">Forbidden. User is already confirmed.</response>
    /// <response code="404">Not found. User does not exist.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPatch("confirm")]
    [EnableRateLimiting(RateLimits.AUTH_REGISTER)]
    [Consumes("application/json")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest),
     TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmRegistration([FromBody] ConfirmRegisterRequestBody request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest, string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }
            
            // Find the user by ID
            CustomUser? user = await _dbContext.FindUserAsync(x => x.Id == request.UserId);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.BadRequest, "User does not exist.");

            // Check if the user's email is already confirmed
            if (user.EmailConfirmed)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "The user is already confirmed.");

            // Validate the confirmation token
            var claim = _dbContext.FindUserClaim(x =>
                x.UserId == request.UserId && x.ClaimType == CustomClaimTypes.EmailConfirmationToken &&
                x.ClaimValue == request.ConfirmationToken);
            if (claim == null)
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid confirmation token");

            // Remove the confirmation token claim and update the user's email confirmation status
            await _dbContext.RemoveUserClaimAsync(claim);
            user.EmailConfirmed = true;
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();

            // Send a confirmation email to the user
            await _emailService.SendEmailAsync(user.Email, user.UserName, "Account Confirmation",
                "Your account has been confirmed<br/>Thank you for confirming your account. You can now log in.");

            return ReturnResponseCode(HttpStatusCode.OK, "User confirmed successfully");
        }
        catch (Exception ex)
        {
            // Log critical errors and return an internal server error response
            Logger.LogCritical("Error during email confirmation: {Message}", ex);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred");
        }
    }

    /// <summary>
    /// Sends a confirmation email to the user with a confirmation token.
    /// </summary>
    /// <param name="user">The user to send the confirmation email to.</param>
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
        
        string confirmationLink = $"{_settings.WebsiteUrl}/register/confirm?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        await _emailService.SendEmailAsync(user.Email, user.UserName, "Registration Confirmation",
            $"Confirm your account by clicking the button below, or by copying and pasting the following link into your browser: {confirmationLink}<br/><br/>", 
            confirmationLink, 
            "Confirm Account");
    }
}