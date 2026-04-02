using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Helpers;

namespace Tavstal.MesterMC.Api.Controllers.Auth;

/// <summary>
/// Controller for managing two-factor authentication (2FA) operations.
/// </summary>
[ApiController]
[Route("/2fa")]
[Tags("Authentication: 2FA")]
public class TwoFactorController : CustomControllerBase {
    
    private readonly CustomUserManager _userManager;
    private readonly CustomSignInManager _signInManager;
    private readonly CustomDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly Settings _settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TwoFactorController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="userManager">Custom user manager for user operations.</param>
    /// <param name="dbContext">Database context for accessing user data.</param>
    /// <param name="emailService">Service for sending emails.</param>
    /// <param name="settings">Application settings.</param>
    public TwoFactorController(ILogger<TwoFactorController> logger, CustomUserManager userManager, CustomSignInManager signInManager, CustomDbContext dbContext, IEmailService emailService, Settings settings) : base(logger, settings)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _emailService = emailService;
        _settings = settings;
    }
    
    /// <summary>
    /// Enables two-factor authentication for the authenticated user.
    /// </summary>
    /// <param name="twoFactorCode">The 2FA code provided by the user.</param>
    /// <response code="200">Two-factor authentication enabled successfully.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Two-factor authentication is already enabled.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPatch("enable")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EnableTwoFactorAuthAsync([BindRequired, StringLength(6)] string twoFactorCode)
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
            
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (user.TwoFactorEnabled)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is already enabled.");
            
            _signInManager.TwoFactorAuthenticatorSignInAsync(twoFactorCode, )
            if (!_userManager.VerifyTwoFactorToken(user, twoFactorCode))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            
            user.TwoFactorEnabled = true;
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();
            
            return ReturnResponseCode(HttpStatusCode.OK, "Two-factor authentication enabled.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to enable 2FA.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Disables two-factor authentication for the authenticated user.
    /// </summary>
    /// <param name="twoFactorCode">The 2FA code provided by the user.</param>
    /// <response code="200">Two-factor authentication disabled successfully.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Two-factor authentication is not enabled.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPatch("disable")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DisableTwoFactorAuthAsync([BindRequired, StringLength(6)] string twoFactorCode)
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
            
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!user.TwoFactorEnabled)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is not enabled.");
            
            if (!await _userManager.VerifyTwoFactorCode(user, "TwoFactorSecret", twoFactorCode))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            
            user.TwoFactorEnabled = false;
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();
            
            return ReturnResponseCode(HttpStatusCode.OK, "Two-factor authentication disabled.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to disable 2FA.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Generates a new 2FA secret for the authenticated user.
    /// </summary>
    /// <response code="200">2FA secret generated successfully.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Two-factor authentication is already enabled.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPatch("generate")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateCodeAsync()
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (user.TwoFactorEnabled)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is already enabled.");

            string rawSecret = await _userManager.GenerateTwoFactorTokenAsync(user, "TwoFactorSecret");
            
            return ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                userId = user.Id,
                email = user.Email,
                secret = rawSecret,
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to generate 2FA secret.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Regenerates recovery codes for the authenticated user.
    /// </summary>
    /// <response code="200">Recovery codes regenerated successfully.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Two-factor authentication is not enabled.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPatch("regenerate/recovery")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegenerateRecoveryCodesAsync()
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            var recoveryCodes = await _dbContext.GetUserBackupCodeAsync(x => x.UserId == user.Id);
            foreach (var code in recoveryCodes) 
                await _dbContext.RemoveUserBackupCodeAsync(code);
            var newCodes = TokenHelper.GenerateRecoveryCodes();
            foreach (var code in newCodes)
            {
                await _dbContext.AddUserBackupCodeAsync(new UserBackupCode
                {
                    UserId = user.Id,
                    HashedCode = StringChiper.GetEncryptedHash(code, _settings.EncryptionKey),
                    CreateAt = DateTime.UtcNow
                });
            }
            
            await _dbContext.SaveChangesAsync();
            
            return ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                userId = user.Id,
                email = user.Email,
                recoveryCodes = newCodes
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to regenerate recovery codes.");
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
}