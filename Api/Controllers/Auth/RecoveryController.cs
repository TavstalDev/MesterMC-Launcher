using System.Globalization;
using System.Net;
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

[Route("/recovery")]
public class RecoveryController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly EmailService _emailService;
    private readonly Settings _settings;
    // TODO: Test Recovery System
    
    public RecoveryController(IConfiguration configuration, ILogger logger, CustomUserManager userManager, CustomDbContext dbContext, EmailService emailService, Settings settings)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
        _emailService = emailService;
        _settings = settings;
    }
    
    
    [HttpPost("request")]
    [TextResponse(StatusCodes.Status201Created), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RequestRecoveryAsync([BindRequired] string email)
    {
        try
        {
            CustomUser? user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");
            
            if (!user.EmailConfirmed)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Email is not confirmed.");

            var userClaim = _dbContext.FindUserClaim(x =>
                x.UserId == user.Id && x.ClaimType == CustomClaimTypes.EmailRecoveryExpiration);
            if (userClaim != null)
            {
                DateTime delayDate = DateTime.Parse(userClaim.ClaimValue!);
                if (delayDate > DateTimeOffset.UtcNow)
                    return this.ReturnResponseCode(HttpStatusCode.Forbidden, "You must wait before requesting another recovery email.");
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
            
            await _emailService.SendEmailAsync(user.Email, "Account Recovery",
                $"<h1>Account Recovery</h1><p>To recover your account, please use the following link: " +
                $"<strong>{_settings.WebsiteUrl}/reset-password?recoveryToken={recoveryToken}</strong></p><p>The link is valid for 15 minutes.</p>");
            
            return this.ReturnResponseCode(HttpStatusCode.Created, "Recovery email sent successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
    
    [HttpPost("password")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecoverPasswordAsync([BindRequired, FromBody] RecoverPasswordRequestBody request)
    {
        try
        {
            CustomUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");
            
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
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Too many recovery attempts. Please try again later."); 
                
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
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid recovery token.");
            
            var expirationClaim = _dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.EmailRecoveryExpiration && x.UserId == user.Id);
            if (expirationClaim == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "Failed to find expiration claim.");
            
            DateTime expirationDate = DateTime.Parse(expirationClaim.ClaimValue!);
            if (expirationDate < DateTimeOffset.UtcNow)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Recovery token has expired.");
            
            user.PasswordHash = StringChiper.GetEncryptedSha256Hash(request.NewPassword, _settings.EncryptionKey);
            await _dbContext.UpdateUserAsync(user);

            await _dbContext.RemoveUserClaimAsync(recoveryTokenClaim);
            await _dbContext.RemoveUserClaimAsync(expirationClaim);

            if (request.LogoutEverywhere)
                await _dbContext.ClearUserLoginsAsync(user.Id);
            
            await _dbContext.SaveChangesAsync();
            return this.ReturnResponseCode(HttpStatusCode.OK, "Password reset successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
    
    [HttpPost("2fa")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status404NotFound), TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RecoverTwoFactorAsync([BindRequired, FromBody] RecoverTwoFactorRequestBody request)
    {
        try
        {
            CustomUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");
            
            if (!user.EmailConfirmed)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Email is not confirmed.");
            
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
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Too many recovery attempts. Please try again later."); 
                
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
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid backup code.");

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            await _dbContext.UpdateUserAsync(user);

            await _dbContext.RemoveUserClaimAsync(backupCodeClaim);
            
            if (request.LogoutEverywhere)
                await _dbContext.ClearUserLoginsAsync(user.Id);
            
            await _dbContext.SaveChangesAsync();
            return this.ReturnResponseCode(HttpStatusCode.OK, "2FA reset successful.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
}