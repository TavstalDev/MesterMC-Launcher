using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using OtpNet;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Claims;
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
    private readonly CustomDbContext _dbContext;
    private readonly EmailService _emailService;
    private readonly Settings _settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="TwoFactorController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="userManager">Custom user manager for user operations.</param>
    /// <param name="dbContext">Database context for accessing user data.</param>
    /// <param name="emailService">Service for sending emails.</param>
    /// <param name="settings">Application settings.</param>
    public TwoFactorController(ILogger<TwoFactorController> logger, CustomUserManager userManager, CustomDbContext dbContext, EmailService emailService, Settings settings) : base(logger)
    {
        _userManager = userManager;
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
    /// <response code="500">Internal server error. Unexpected error occurred.</response>
    [HttpPatch("enable")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EnableTwoFactorAuthAsync([BindRequired] string twoFactorCode)
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (user.TwoFactorEnabled)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is already enabled.");
            
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(twoFactorCode, out _, new VerificationWindow(2, 2)))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            
            user.TwoFactorEnabled = true;
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();
            
            return ReturnResponseCode(HttpStatusCode.OK, "Two-factor authentication enabled.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
    
    /// <summary>
    /// Disables two-factor authentication for the authenticated user.
    /// </summary>
    /// <param name="twoFactorCode">The 2FA code provided by the user.</param>
    /// <response code="200">Two-factor authentication disabled successfully.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Two-factor authentication is not enabled.</response>
    /// <response code="500">Internal server error. Unexpected error occurred.</response>
    [HttpPatch("disable")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DisableTwoFactorAuthAsync([BindRequired] string twoFactorCode)
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!user.TwoFactorEnabled)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is not enabled.");
            
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(twoFactorCode, out _, new VerificationWindow(2, 2)))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            
            user.TwoFactorEnabled = false;
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();
            
            return ReturnResponseCode(HttpStatusCode.OK, "Two-factor authentication disabled.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
    
    /// <summary>
    /// Generates a new 2FA secret for the authenticated user.
    /// </summary>
    /// <response code="200">2FA secret generated successfully.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Two-factor authentication is already enabled.</response>
    /// <response code="500">Internal server error. Unexpected error occurred.</response>
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

            user.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();
            
            return ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                userId = user.Id,
                email = user.Email,
                secret = user.TwoFactorSecret,
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
    
    /// <summary>
    /// Regenerates recovery codes for the authenticated user.
    /// </summary>
    /// <response code="200">Recovery codes regenerated successfully.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Two-factor authentication is not enabled.</response>
    /// <response code="500">Internal server error. Unexpected error occurred.</response>
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

            var recoveryCodeClaims = _dbContext.GetUserClaims(x => x.UserId == user.Id && x.ClaimType == CustomClaimTypes.TwoFactorRecoveryCode);
            foreach (var claim in recoveryCodeClaims) 
                _dbContext.Remove(claim);
            var recoveryCodes = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                var recoveryCode = TokenHelper.GenerateRecoveryCode();
                var claim = new CustomUserClaim
                {
                    UserId = user.Id,
                    ClaimType = CustomClaimTypes.TwoFactorRecoveryCode,
                    ClaimValue = StringChiper.GetEncryptedSha256Hash(recoveryCode, _settings.EncryptionKey)
                };
                recoveryCodes.Add(recoveryCode);
                await _dbContext.AddUserClaimAsync(claim);
            }
            
            await _dbContext.SaveChangesAsync();
            
            return ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                userId = user.Id,
                email = user.Email,
                recoveryCodes
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
}