using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OtpNet;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;
using Tavstal.MesterMC.Api.Utils.Helpers;

namespace Tavstal.MesterMC.Api.Controllers.Auth;

[ApiController]
[Route("/2fa")]
[Tags("Authentication: 2FA")]
public class TwoFactorController : CustomControllerBase {

    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly EmailService _emailService;
    private readonly Settings _settings;
    // TODO: Test TwoFactor Auth System
    
    public TwoFactorController(ILogger<TwoFactorController> logger, CustomUserManager userManager, CustomDbContext dbContext, EmailService emailService, Settings settings)
    {
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
        _emailService = emailService;
        _settings = settings;
    }
    
    [HttpPatch("enable")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> EnableTwoFactorAuthAsync([BindRequired] string twoFactorCode)
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (user.TwoFactorEnabled)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is already enabled.");
            
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(twoFactorCode, out _, new VerificationWindow(2, 2)))
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            
            user.TwoFactorEnabled = true;
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();
            
            return this.ReturnResponseCode(HttpStatusCode.OK, "Two-factor authentication enabled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
    
    [HttpPatch("disable")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DisableTwoFactorAuthAsync([BindRequired] string twoFactorCode)
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (!user.TwoFactorEnabled)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is not enabled.");
            
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(twoFactorCode, out _, new VerificationWindow(2, 2)))
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            
            user.TwoFactorEnabled = false;
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();
            
            return this.ReturnResponseCode(HttpStatusCode.OK, "Two-factor authentication disabled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
    
    [HttpPatch("generate")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GenerateCodeAsync()
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

            if (user.TwoFactorEnabled)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is already enabled.");

            user.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.UpdateUserAsync(user);
            await _dbContext.SaveChangesAsync();
            
            return this.ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                userId = user.Id,
                email = user.Email,
                secret = user.TwoFactorSecret,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
    
    [HttpPatch("regenerate/recovery")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), 
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegenerateRecoveryCodesAsync()
    {
        try
        {
            CustomUser? user = await GetCurrentUserAsync(_userManager);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");

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
            
            return this.ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                userId = user.Id,
                email = user.Email,
                recoveryCodes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
}