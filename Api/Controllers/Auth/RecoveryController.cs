using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Bodies.Auth;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Helpers;

namespace Tavstal.MesterMC.Api.Controllers.Auth;

/// <summary>
/// Controller for handling account recovery operations, including password and 2FA recovery.
/// </summary>
[ApiController]
[Route("/recovery")]
[Tags("Authentication: Recovery")]
public class RecoveryController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly Settings _settings;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="RecoveryController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="userManager">Custom user manager for user operations.</param>
    /// <param name="dbContext">Database context for accessing user data.</param>
    /// <param name="emailService">Service for sending emails.</param>
    /// <param name="settings">Application settings.</param>
    public RecoveryController(ILogger<RecoveryController> logger, CustomUserManager userManager, CustomDbContext dbContext, IEmailService emailService, Settings settings) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _emailService = emailService;
        _settings = settings;
    }
    
    
    /// <summary>
    /// Handles requests to send a recovery email to the user.
    /// </summary>
    /// <param name="email">The email address of the user requesting recovery.</param>
    /// <response code="201">Recovery email sent successfully.</response>
    /// <response code="403">Forbidden. Email is not confirmed or recovery request is too frequent.</response>
    /// <response code="404">Not found. User does not exist.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPost("request")]
    [EnableRateLimiting(RateLimits.AUTH_RESET)]
    [TextResponse(StatusCodes.Status201Created), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestRecoveryAsync([BindRequired] string email)
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
            
            CustomUser? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");
            
            if (!user.EmailConfirmed)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Email is not confirmed.");

            var userClaim = _dbContext.FindUserClaim(x =>
                x.UserId == user.Id && x.ClaimType == CustomClaimTypes.EmailRecoveryExpiration);
            if (userClaim != null)
            {
                DateTime delayDate = DateTime.Parse(userClaim.ClaimValue!);
                if (delayDate > DateTimeOffset.UtcNow)
                    return ReturnResponseCode(HttpStatusCode.Forbidden, "You must wait before requesting another recovery email.");
            }

            await _dbContext.SetUserClaimAsync(new CustomUserClaim
            {
                UserId = user.Id,
                ClaimType = CustomClaimTypes.EmailRecoveryExpiration,
                ClaimValue = DateTimeOffset.UtcNow.AddMinutes(15).ToString(CultureInfo.InvariantCulture)
            });

            var recoveryToken = DatabaseHelper.GenerateRecoveryToken(_dbContext);
            await _dbContext.SetUserClaimAsync(new CustomUserClaim
            {
                UserId = user.Id,
                ClaimType = CustomClaimTypes.EmailRecoveryToken,
                ClaimValue = recoveryToken
            });
            
            await _dbContext.SaveChangesAsync();
            
            string recoveryLink = $"{_settings.WebsiteUrl}/reset-password?recoveryToken={recoveryToken}";
            await _emailService.SendEmailAsync(user.Email, user.UserName, "Account Recovery", 
                $"Click the button below or copy and paste the following link into your browser to recover your account: {recoveryLink}" +
                "<br><br>The link is valid for 15 minutes." +
                "<br><br>If you did not request this recovery email, you can ignore it.", 
                recoveryLink, 
                "Recover Account");
            
            return ReturnResponseCode(HttpStatusCode.Created, "Recovery email sent successfully.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Handles password recovery requests.
    /// </summary>
    /// <param name="request">The recovery request body containing email, recovery token, and new password.</param>
    /// <response code="200">Password reset successful.</response>
    /// <response code="403">Forbidden. Too many recovery attempts or token expired.</response>
    /// <response code="404">Not found. User or required claims do not exist.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPost("password")]
    [EnableRateLimiting(RateLimits.AUTH_RESET)]
    [Consumes("application/json")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecoverPasswordAsync([FromBody] RecoverPasswordRequestBody request)
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
            
            CustomUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");
            
            #region Bruteforce protection
            var recoveryAttemptExpirationClaim = _dbContext.FindUserClaim(x =>
                x.UserId == user.Id && x.ClaimType == CustomClaimTypes.EmailRecoveryExpiration);
            if (recoveryAttemptExpirationClaim != null && DateTime.TryParse(recoveryAttemptExpirationClaim.ClaimValue, out DateTime recoveryAttemptExpiration))
            {
                // Reset the attempts
                if (recoveryAttemptExpiration < DateTimeOffset.UtcNow)
                {
                    recoveryAttemptExpirationClaim.ClaimValue = DateTimeOffset.UtcNow.AddMinutes(15).ToString(CultureInfo.InvariantCulture);
                    await _dbContext.UpdateUserClaimAsync(recoveryAttemptExpirationClaim);
                    await _dbContext.SetUserClaimAsync(new CustomUserClaim
                    {
                        UserId = user.Id,
                        ClaimType = CustomClaimTypes.EmailRecoveryAttempt,
                        ClaimValue = "0"
                    });
                }
            }
            else
            {
                await _dbContext.SetUserClaimAsync(new CustomUserClaim
                {
                    UserId = user.Id,
                    ClaimType = CustomClaimTypes.EmailRecoveryAttemptExpiration,
                    ClaimValue = DateTimeOffset.UtcNow.AddMinutes(15).ToString(CultureInfo.InvariantCulture)
                });
            }
            
            
            var attemptClaim = _dbContext.FindUserClaim(x =>
                x.UserId == user.Id && x.ClaimType == CustomClaimTypes.EmailRecoveryAttempt);
            if (attemptClaim == null)
            {
                attemptClaim = await _dbContext.SetUserClaimAsync(new CustomUserClaim
                {
                    UserId = user.Id,
                    ClaimType = CustomClaimTypes.EmailRecoveryAttempt,
                    ClaimValue = "0"
                });
            }

            if (!int.TryParse(attemptClaim?.ClaimValue, out int attempts))
                attempts = 0;

           
            if (attempts > 3) 
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Too many recovery attempts. Please try again later."); 
                
            await _dbContext.SetUserClaimAsync(new CustomUserClaim
            {
                UserId = user.Id,
                ClaimType = CustomClaimTypes.EmailRecoveryAttempt,
                ClaimValue = (attempts + 1).ToString()
            });
            await _dbContext.SaveChangesAsync();
            #endregion
            
            var recoveryTokenClaim = _dbContext.FindUserClaim(x => x.UserId == user.Id && x.ClaimType == CustomClaimTypes.EmailRecoveryToken && x.ClaimValue == request.RecoveryToken);
            if ( recoveryTokenClaim == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid recovery token.");
            
            var expirationClaim = _dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.EmailRecoveryExpiration && x.UserId == user.Id);
            if (expirationClaim == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "Failed to find expiration claim.");
            
            DateTime expirationDate = DateTime.Parse(expirationClaim.ClaimValue!);
            if (expirationDate < DateTimeOffset.UtcNow)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Recovery token has expired.");
            
            user.PasswordHash = StringChiper.GetEncryptedSha256Hash(request.NewPassword, _settings.EncryptionKey);
            await _dbContext.UpdateUserAsync(user);

            await _dbContext.RemoveUserClaimAsync(recoveryTokenClaim);
            await _dbContext.RemoveUserClaimAsync(expirationClaim);

            if (request.LogoutEverywhere)
                await _dbContext.ClearUserLoginsAsync(user.Id);
            
            await _dbContext.SaveChangesAsync();
            return ReturnResponseCode(HttpStatusCode.OK, "Password reset successful.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
    
    /// <summary>
    /// Handles requests to recover two-factor authentication (2FA) settings.
    /// </summary>
    /// <param name="request">The recovery request body containing email and backup code.</param>
    /// <response code="200">2FA reset successful.</response>
    /// <response code="403">Forbidden. Too many recovery attempts.</response>
    /// <response code="404">Not found. User or required claims do not exist.</response>
    /// <response code="500">Internal server error. An unknown error occurred while processing the request.</response>
    [HttpPost("2fa")]
    [EnableRateLimiting(RateLimits.AUTH_RESET)]
    [Consumes("application/json")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecoverTwoFactorAsync([FromBody] RecoverTwoFactorRequestBody request)
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
            
            CustomUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");
            
            if (!user.EmailConfirmed)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Email is not confirmed.");
            
            #region Bruteforce protection
            var recoveryAttemptExpirationClaim = _dbContext.FindUserClaim(x =>
                x.UserId == user.Id && x.ClaimType == CustomClaimTypes.TwoFactorRecoveryAttemptExpiry);
            if (recoveryAttemptExpirationClaim != null && DateTime.TryParse(recoveryAttemptExpirationClaim.ClaimValue, out DateTime recoveryAttemptExpiration))
            {
                // Reset the attempts
                if (recoveryAttemptExpiration < DateTimeOffset.UtcNow)
                {
                    recoveryAttemptExpirationClaim.ClaimValue = DateTimeOffset.UtcNow.AddMinutes(15).ToString(CultureInfo.InvariantCulture);
                    await _dbContext.UpdateUserClaimAsync(recoveryAttemptExpirationClaim);
                    await _dbContext.SetUserClaimAsync(new CustomUserClaim
                    {
                        UserId = user.Id,
                        ClaimType = CustomClaimTypes.TwoFactorRecoveryAttemptCount,
                        ClaimValue = "0"
                    });
                }
            }
            else
            {
                await _dbContext.SetUserClaimAsync(new CustomUserClaim
                {
                    UserId = user.Id,
                    ClaimType = CustomClaimTypes.TwoFactorRecoveryAttemptExpiry,
                    ClaimValue = DateTimeOffset.UtcNow.AddMinutes(15).ToString(CultureInfo.InvariantCulture)
                });
            }
            
            
            var attemptClaim = _dbContext.FindUserClaim(x =>
                x.UserId == user.Id && x.ClaimType == CustomClaimTypes.TwoFactorRecoveryAttemptCount) ?? await _dbContext.SetUserClaimAsync(new CustomUserClaim
            {
                UserId = user.Id,
                ClaimType = CustomClaimTypes.TwoFactorRecoveryAttemptCount,
                ClaimValue = "0"
            });

            if (!int.TryParse(attemptClaim?.ClaimValue, out int attempts))
                attempts = 0;

           
            if (attempts > 3) 
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Too many recovery attempts. Please try again later."); 
                
            await _dbContext.SetUserClaimAsync(new CustomUserClaim
            {
                UserId = user.Id,
                ClaimType = CustomClaimTypes.TwoFactorRecoveryAttemptCount,
                ClaimValue = (attempts + 1).ToString()
            });
            await _dbContext.SaveChangesAsync();
            #endregion
            
            var backupCodeClaim = _dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.TwoFactorRecoveryCode && x.ClaimValue == request.BackupCode && x.UserId == user.Id);
            if (backupCodeClaim == null)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid backup code.");

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _dbContext.UpdateUserAsync(user);

            await _dbContext.RemoveUserClaimAsync(backupCodeClaim);
            
            if (request.LogoutEverywhere)
                await _dbContext.ClearUserLoginsAsync(user.Id);
            
            await _dbContext.SaveChangesAsync();
            return ReturnResponseCode(HttpStatusCode.OK, "2FA reset successful.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
}