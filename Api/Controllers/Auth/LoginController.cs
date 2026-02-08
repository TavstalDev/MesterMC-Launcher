using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OtpNet;
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

public class LoginController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly EmailService _emailService;
    private readonly JwtSettings _jwtSettings;
    
    public LoginController(IConfiguration configuration, ILogger<LoginController> logger, CustomDbContext dbContext,
        CustomUserManager userManager, EmailService emailService, JwtSettings jwtSettings)
    {
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;
        _userManager = userManager;
        _emailService = emailService;
        _jwtSettings = jwtSettings;
    }
    
    [HttpPost("/login")]
    [JsonResponse(StatusCodes.Status200OK, typeof(LoginResponse)), TextResponse(StatusCodes.Status401Unauthorized),
     TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginAsync([BindRequired, FromBody] LoginRequestBody request)
    {
        try
        {
            var normalizedEmail = request.Email.Normalize();
            CustomUser? user = await _dbContext.FindUserAsync(x => (x.NormalizedEmail.Equals(normalizedEmail) || x.NormalizedUserName.Equals(normalizedEmail)));
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

            // Check if the user is locked out
            if (user.LockoutEnabled)
            {
                if (user.LockoutEnd > DateTime.Now)
                    return this.ReturnJson(HttpStatusCode.Locked, new
                    {
                        message = string.IsNullOrEmpty(user.LockoutReason) ? "User is locked out" : user.LockoutReason,
                        lockoutExpires = user.LockoutEnd.ToString()
                    });
            
                user.LockoutEnabled = false;
                user.LockoutReason = string.Empty;
                await _dbContext.UpdateUserAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            
            // Check password
            if (user.PasswordHash != StringChiper.GetEncryptedSha256Hash(request.Password, _configuration.GetValue<string>("EncryptionKey")!))
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid password.");

            // Check two-factor authentication
            if (user.TwoFactorEnabled)
            {
                if (request.TwoFactorCode == null)
                {
                    string sessionSecret = TokenHelper.GenerateTwoFactorSessionToken();
                    DateTime expiry = DateTime.Now.AddMinutes(5);

                    await _dbContext.SetUserClaimAsync(new CustomUserClaim
                    {
                        UserId = user.Id,
                        ClaimType = CustomClaimTypes.TwoFactorSessionToken,
                        ClaimValue = sessionSecret
                    });
                    
                    await _dbContext.SetUserClaimAsync(new CustomUserClaim
                    {
                        UserId = user.Id,
                        ClaimType = CustomClaimTypes.TwoFactorSessionExpiration,
                        ClaimValue = expiry.ToString(CultureInfo.InvariantCulture)
                    });

                    await _dbContext.SaveChangesAsync();
                    
                    Response.Cookies.Append("mmc-twofactor-session", sessionSecret, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // only over HTTPS
                        SameSite = SameSiteMode.None, // required for cross-origin
                        Expires = expiry
                    });
                    
                    return this.ReturnJson(HttpStatusCode.Redirect, new
                    {
                        message = "Redirect to 2FA page",
                        email = user.Email,
                        url = $"{_configuration.GetValue<string>("Servers:Website")}/2fa?rememberMe={request.RememberMe}"
                    });
                }


                var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
                if (!totp.VerifyTotp(request.TwoFactorCode, out _, new VerificationWindow(2, 2)))
                    return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            }

            string ipv4 = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
            string ipv6 = HttpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "::1";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string operatingSystem = HttpHelper.GetOperatingSystem(userAgent);
            string browser = HttpHelper.GetBrowser(userAgent);
            
            IpInfo ipInfo = await DatabaseHelper.GetIpInformation(ipv4);
            DateTime expireDate = request.RememberMe ? DateTime.Now.AddDays(7) : DateTime.Now.AddMinutes(60);
            var userToken = await _dbContext.AddUserTokenAsync(
                new CustomUserToken(
                    user.Id, 
                    "AccessToken", 
                    TokenHelper.GenerateJwtToken(_jwtSettings.EncryptionKey, _jwtSettings.Issuer, _jwtSettings.Audience, expireDate), 
                    "MesterMC", 
                    DateTime.Now), 
                true);
            
            var userLogin = await _dbContext.AddUserLoginAsync(new CustomUserLogin(user.Id,  userToken.Id, "MesterMC", 
                "MesterMC", ipv4, ipv6, ipInfo, operatingSystem, browser, DateTime.Now, expireDate), true);
            
            Response.Cookies.Append("mmc-token", userToken.Value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // only over HTTPS
                SameSite = SameSiteMode.None, // required for cross-origin
                Expires = userLogin.ExpireDate
            });
            
            return this.ReturnJson(HttpStatusCode.OK, new LoginResponse
            {
                Message = "Login successful",
                UserId = userToken.UserId,
                Expires = userLogin.ExpireDate.ToString(CultureInfo.InvariantCulture)
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Error during login: {Message}", ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }

    
    [HttpPatch("/login/2fa")]
    [JsonResponse(StatusCodes.Status200OK, typeof(LoginResponse)), TextResponse(StatusCodes.Status401Unauthorized),
     TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginTwoFactorAsync([BindRequired, FromBody] LoginTFASessionRequestBody request)
    {
        try
        {
            if (!Request.Cookies.TryGetValue("mmc-twofactor-session", out var cookieValue) || string.IsNullOrEmpty(cookieValue))
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid or missing session cookie.");
            
            var sessionClaim = _dbContext.FindUserClaim(x=> x.ClaimType == CustomClaimTypes.TwoFactorSessionToken && x.ClaimValue == cookieValue);
            if (sessionClaim == null)
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid session secret.");
            
            var expiryClaim = _dbContext.FindUserClaim(x=> x.ClaimType == CustomClaimTypes.TwoFactorSessionExpiration && x.UserId == sessionClaim.UserId);
            if (expiryClaim == null || !DateTime.TryParse(expiryClaim.ClaimValue, out DateTime expiry))
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid session secret expiry.");
            
            if (DateTime.Now > expiry)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "Session token expired.");
            
            CustomUser? user = await _dbContext.FindUserAsync(x => x.Id == sessionClaim.UserId);
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

            if (user.LockoutEnabled)
            {
                if (user.LockoutEnd > DateTime.Now)
                    return this.ReturnJson(HttpStatusCode.Locked, new
                    {
                        message = string.IsNullOrEmpty(user.LockoutReason) ? "User is locked out" : user.LockoutReason,
                        lockoutEnd = user.LockoutEnd.ToString()
                    });
            
                user.LockoutEnabled = false;
                user.LockoutReason = string.Empty;
                await _dbContext.UpdateUserAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            
            var sessionAttemptClaim = _dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.TwoFactorSessionAttempt && x.UserId == user.Id);
            if (sessionAttemptClaim == null)
                return this.ReturnResponseCode(HttpStatusCode.NotFound, "Session attempt claim not found.");
            
            int sessionAttempt = int.Parse(sessionAttemptClaim.ClaimValue!);
            if (sessionAttempt >= 3)
                return this.ReturnResponseCode(HttpStatusCode.Forbidden, "To many failed attempts. Please try reauthorizing again.");
            
            var totp = new Totp(Base32Encoding.ToBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(request.TwoFactorCode, out _, new VerificationWindow(2, 2)))
            {
                sessionAttempt += 1;
                await _dbContext.SetUserClaimAsync(new CustomUserClaim
                {
                    ClaimType = CustomClaimTypes.TwoFactorSessionAttempt,
                    ClaimValue = sessionAttempt.ToString(),
                    UserId = user.Id
                });
                await _dbContext.SaveChangesAsync();
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            }

            string ipv4 = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
            string ipv6 = HttpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "::1";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string operatingSystem = HttpHelper.GetOperatingSystem(userAgent);
            string browser = HttpHelper.GetBrowser(userAgent);
            
            IpInfo ipInfo = await DatabaseHelper.GetIpInformation(ipv4);
            DateTime expireDate = request.RememberMe ? DateTime.Now.AddDays(7) : DateTime.Now.AddMinutes(60);
            var userToken = await _dbContext.AddUserTokenAsync(new CustomUserToken(
                user.Id, 
                "AccessToken", 
                TokenHelper.GenerateJwtToken(_jwtSettings.EncryptionKey, _jwtSettings.Issuer, _jwtSettings.Audience, expireDate), 
                "MesterMC", 
                DateTime.Now), 
                true);
            
            var userLogin = await _dbContext.AddUserLoginAsync(new CustomUserLogin(user.Id,  userToken.Id, "MesterMC", 
                "MesterMC", ipv4, ipv6, ipInfo, operatingSystem, browser, DateTime.Now, expireDate));

            await _dbContext.RemoveUserClaimAsync(sessionClaim);
            await _dbContext.RemoveUserClaimAsync(expiryClaim);
            
            await _dbContext.SaveChangesAsync();
            
            return this.ReturnJson(HttpStatusCode.OK, new LoginResponse
            {
                Message = "Login successful",
                UserId = userToken.UserId,
                Token = userToken.Value,
                Expires = userLogin.ExpireDate.ToString(CultureInfo.InvariantCulture)
            });
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Error during login: {Message}", ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }

    
    [HttpGet("/login/check")]
    [TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status500InternalServerError)]
    [JsonResponse(StatusCodes.Status200OK, typeof(LoggedInResponse))]
    public async Task<IActionResult> CheckIfLoggedInAsync()
    {
        try
        {
            var user = await _userManager.GetUserByAuthenticationStringAsync(this.GetAuthenticationToken());
            if (user == null)
                return this.ReturnResponseCode(HttpStatusCode.Unauthorized, "The user is not authenticated.");
            
            List<CustomRole> roles = _userManager.GetUserRoles(user.Id);
            var claims = _userManager.GetAllClaimsOfUser(user.Id);

            bool hasAvatar = !string.IsNullOrEmpty(user.AvatarPath);
            return this.ReturnJson(HttpStatusCode.OK, new LoggedInResponse
            {
                UserId = user.Id,
                Username = user.UserName,
                DisplayName = user.DisplayName ?? user.UserName,
                Email = user.Email,
                HasAvatar = hasAvatar,
                Avatar = hasAvatar ? $"{_configuration.GetValue<string>("Servers:API")}/users/{user.Id}/avatar" : "",
                Roles = roles,
                Claims = claims,
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }


    [HttpPost("/logout")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LogoutAsync(string? token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
                token = this.GetAuthenticationToken();
            
            if (string.IsNullOrEmpty(token))
                return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid token.");
            
            CustomUserToken? userToken = _dbContext.FindUserToken(x => x.Value == token);
            if (userToken == null)
                return this.ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid token.");
            
            CustomUserLogin? userLogin = _dbContext.FindUserLogin(x => x.UserId == userToken.UserId && x.ProviderKey == userToken.Id);
            if (userLogin != null)
                await _dbContext.RemoveUserLoginAsync(userLogin);
            
            await _dbContext.RemoveUserTokenAsync(userToken);
            await _dbContext.SaveChangesAsync();
            return SignOut();
        }
        catch (Exception ex)
        {
            _logger.LogCritical("Error during logout: {Message}", ex.Message);
            return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Unexpected error occurred.");
        }
    }
}