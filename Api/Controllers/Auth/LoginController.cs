using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OtpNet;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Bodies.Auth;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Helpers;

namespace Tavstal.MesterMC.Api.Controllers.Auth;

/// <summary>
/// Controller responsible for handling login-related authentication endpoints.
/// </summary>
[ApiController]
[Tags("Authentication: Login")]
public class LoginController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly MemoryCacheService _memoryCacheService;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging operations.</param>
    /// <param name="dbContext">The database context for accessing user-related data.</param>
    /// <param name="userManager">The user manager for managing user authentication and roles.</param>
    /// <param name="emailService">The email service for sending emails.</param>
    /// <param name="memoryCacheService">Service for caching launcher data.</param>
    /// <param name="settings">The application settings.</param>
    public LoginController(ILogger<LoginController> logger, CustomDbContext dbContext,
        CustomUserManager userManager, IEmailService emailService, MemoryCacheService memoryCacheService, Settings settings) : base(logger, settings)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _emailService = emailService;
        _memoryCacheService = memoryCacheService;
    }
    
    /// <summary>
    /// Handles user login requests.
    /// </summary>
    /// <param name="request">The login request body containing user credentials.</param>
    /// <response code="200">Request successful. Returns authentication result and tokens or session info when applicable.</response>
    /// <response code="302">Redirect required (e.g. to 2FA page). Response body includes redirect URL and related info.</response>
    /// <response code="400">Bad request. Missing or invalid input (e.g. missing token or malformed body).</response>
    /// <response code="401">Unauthorized. Authentication failed (invalid credentials, invalid session token or 2FA code).</response>
    /// <response code="403">Forbidden. Access denied (e.g. expired session token, too many attempts, or account restrictions).</response>
    /// <response code="404">Not found. Requested resource (user, session, token) does not exist.</response>
    /// <response code="423">Locked. Account is locked; includes lockout reason and expiration when applicable.</response>
    /// <response code="500">Internal server error. Unexpected error occurred while processing the request.</response>
    [HttpPost("/login")]
    [EnableRateLimiting(RateLimits.AUTH_LOGIN)]
    [Consumes("application/json")]
    [JsonResponse(StatusCodes.Status200OK, typeof(LoginResponse)), TextResponse(StatusCodes.Status401Unauthorized),
     TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequestBody request)
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
            
            var normalizedEmail = request.Email.Normalize();
            CustomUser? user = await _dbContext.FindUserAsync(x => (x.NormalizedEmail.Equals(normalizedEmail) || x.NormalizedUserName.Equals(normalizedEmail)));
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

            // Check if the user is locked out
            if (user.LockoutEnabled)
            {
                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                    return ReturnJson(new
                    {
                        statusCode = HttpStatusCode.Locked,
                        message = string.IsNullOrEmpty(user.LockoutReason) ? "User is locked out" : user.LockoutReason,
                        lockoutExpires = user.LockoutEnd.ToString()
                    });
            
                user.LockoutEnabled = false;
                user.LockoutReason = string.Empty;
                await _dbContext.UpdateUserAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            
            // Check password
            if (user.PasswordHash != StringChiper.GetEncryptedSha256Hash(request.Password, Settings.EncryptionKey))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid password.");

            // Check two-factor authentication
            if (user.TwoFactorEnabled)
            {
                string sessionToken = TokenHelper.GenerateTwoFactorSessionToken();
                string fingerprint = GetMachineFingerprint(user.Id);
                _memoryCacheService.SetValue($"auth:{fingerprint}:tfa:token", sessionToken, TimeSpan.FromMinutes(5));
                _memoryCacheService.SetValue($"auth:{fingerprint}:tfa:attempts", 0, TimeSpan.FromMinutes(5));

                var expiry = DateTimeOffset.UtcNow.AddMinutes(5);
                Response.Cookies.Append("mmc-userId", user.Id, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // only over HTTPS
                    SameSite = SameSiteMode.None, // required for cross-origin
                    Expires = expiry
                });
                Response.Cookies.Append("mmc-twofactor-session", sessionToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true, // only over HTTPS
                    SameSite = SameSiteMode.None, // required for cross-origin
                    Expires = expiry
                });
                    
                return ReturnJson(new
                {
                    statusCode = HttpStatusCode.Redirect,
                    message = "Redirect to 2FA page",
                    email = user.Email,
                    url = $"{Settings.WebsiteUrl}/2fa?rememberMe={request.RememberMe}"
                });
            }

            string ipv4 = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
            string ipv6 = HttpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "::1";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string operatingSystem = HttpHelper.GetOperatingSystem(userAgent);
            string browser = HttpHelper.GetBrowser(userAgent);
            
            IpInfo ipInfo = await DatabaseHelper.GetIpInformation(ipv4);
            DateTimeOffset expireDate = request.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(60);
            var userToken = await _dbContext.AddUserTokenAsync(
                new CustomUserToken(
                    user.Id, 
                    "AccessToken", 
                    TokenHelper.GenerateJwtToken(Settings.EncryptionKey, Settings.Issuer, Settings.Audience, expireDate), 
                    "MesterMC", 
                    DateTimeOffset.UtcNow), 
                true);
            
            var userLogin = await _dbContext.AddUserLoginAsync(new CustomUserLogin(user.Id,  userToken.Id, "MesterMC", 
                "MesterMC", ipv4, ipv6, ipInfo, operatingSystem, browser, DateTimeOffset.UtcNow, expireDate), true);
            
            Response.Cookies.Append("mmc-token", userToken.Value, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // only over HTTPS
                SameSite = SameSiteMode.None, // required for cross-origin
                Expires = userLogin.ExpireDate
            });
            
            return ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                Message = "Login successful",
                userToken.UserId,
                Expires = userLogin.ExpireDate.ToString(CultureInfo.InvariantCulture)
            });
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Error during login: {Message}", ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, ex.ToString());
        }
    }

    
    /// <summary>
    /// Handles two-factor authentication (2FA) login requests.
    /// </summary>
    /// <param name="request">The 2FA session request body containing the session token and 2FA code.</param>
    /// <response code="200">Request successful. Returns authentication result and tokens.</response>
    /// <response code="401">Unauthorized. Invalid or missing session cookie, session secret, or 2FA code.</response>
    /// <response code="403">Forbidden. Session token expired or too many failed attempts.</response>
    /// <response code="404">Not found. User associated with the session token does not exist.</response>
    /// <response code="423">Locked. User account is locked; includes lockout reason and expiration.</response>
    /// <response code="500">Internal server error. Unexpected error occurred while processing the request.</response>
    [HttpPatch("/login/2fa")]
    [EnableRateLimiting(RateLimits.AUTH_LOGIN)]
    [Consumes("application/json")]
    [JsonResponse(StatusCodes.Status200OK, typeof(LoginResponse)), TextResponse(StatusCodes.Status401Unauthorized),
     TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginTwoFactorAsync([FromBody] LoginTFASessionRequestBody request)
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
            
            if (!Request.Cookies.TryGetValue("mmc-twofactor-session", out var sessionCookie)  || string.IsNullOrEmpty(sessionCookie))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid or missing session cookie.");
            
            if (!Request.Cookies.TryGetValue("mmc-userId", out var userIdCookie) || string.IsNullOrEmpty(userIdCookie))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid or missing userId cookie.");
            
            string fingerprint = GetMachineFingerprint(userIdCookie);
            string tokenKey = $"auth:{fingerprint}:tfa:token";
            if (!_memoryCacheService.TryGetValue(tokenKey, out string? cachedSessionToken) || string.IsNullOrEmpty(cachedSessionToken) || cachedSessionToken != sessionCookie)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid or expired session token.");

            string attemptKey = $"auth:{fingerprint}:tfa:attempts";
            if (!_memoryCacheService.TryGetValue(attemptKey, out int cachedAttempts))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid or expired session token.");
            
            if (cachedAttempts >= 3)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "To many failed attempts. Please try reauthorizing again.");
            
            CustomUser? user = await _dbContext.FindUserAsync(x => x.Id == userIdCookie);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

            if (user.LockoutEnabled)
            {
                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                    return ReturnJson(new
                    {
                        statusCode = HttpStatusCode.Locked,
                        message = string.IsNullOrEmpty(user.LockoutReason) ? "User is locked out" : user.LockoutReason,
                        lockoutEnd = user.LockoutEnd.ToString()
                    });
            
                user.LockoutEnabled = false;
                user.LockoutReason = string.Empty;
                await _dbContext.UpdateUserAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            
            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is not enabled for this user.");

            var totp = new Totp(Encoding.UTF8.GetBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(request.TwoFactorCode, out _, new VerificationWindow(2, 2)))
            {
                _memoryCacheService.SetValue(attemptKey, cachedAttempts + 1);
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            }

            string ipv4 = HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "127.0.0.1";
            string ipv6 = HttpContext.Connection.RemoteIpAddress?.MapToIPv6().ToString() ?? "::1";
            var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
            string operatingSystem = HttpHelper.GetOperatingSystem(userAgent);
            string browser = HttpHelper.GetBrowser(userAgent);
            
            IpInfo ipInfo = await DatabaseHelper.GetIpInformation(ipv4);
            DateTimeOffset expireDate = request.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(60);
            var userToken = await _dbContext.AddUserTokenAsync(new CustomUserToken(
                user.Id, 
                "AccessToken", 
                TokenHelper.GenerateJwtToken(Settings.EncryptionKey, Settings.Issuer, Settings.Audience, expireDate), 
                "MesterMC", 
                DateTimeOffset.UtcNow), 
                true);
            
            var userLogin = await _dbContext.AddUserLoginAsync(new CustomUserLogin(user.Id,  userToken.Id, "MesterMC", 
                "MesterMC", ipv4, ipv6, ipInfo, operatingSystem, browser, DateTimeOffset.UtcNow, expireDate));
            
            await _dbContext.SaveChangesAsync();
            
            _memoryCacheService.RemoveValue(tokenKey);
            _memoryCacheService.RemoveValue(attemptKey);
            return ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                Message = "Login successful",
                userToken.UserId,
                Token = userToken.Value,
                Expires = userLogin.ExpireDate.ToString(CultureInfo.InvariantCulture)
            });
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Error during login: {Message}", ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, ex.ToString());
        }
    }
    
    /// <summary>
    /// Handles launcher login requests.
    /// </summary>
    /// <param name="request">The launcher login request body containing username, password, and optional 2FA code.</param>
    /// <response code="200">Request successful. Returns user session token and expiration details.</response>
    /// <response code="302">Redirect required for two-factor authentication. Includes session token and redirect URL.</response>
    /// <response code="401">Unauthorized. Invalid credentials or two-factor authentication code.</response>
    /// <response code="403">Forbidden. Account is locked or too many failed attempts.</response>
    /// <response code="404">Not found. User does not exist.</response>
    /// <response code="500">Internal server error. Unexpected error occurred while processing the request.</response>
    [HttpPost("/login/launcher")]
    [EnableRateLimiting(RateLimits.AUTH_LOGIN)]
    [Consumes("application/json")]
    [JsonResponse(StatusCodes.Status200OK, typeof(LoginResponse)), TextResponse(StatusCodes.Status401Unauthorized),
     TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginLauncherAsync([FromBody] LauncherLoginRequestBody request)
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
            
            var normalizedUsername = request.Username.Normalize();
            CustomUser? user = await _dbContext.FindUserAsync(x => x.NormalizedUserName.Equals(normalizedUsername));
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

            // Check if the user is locked out
            if (user.LockoutEnabled)
            {
                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                    return ReturnJson(new
                    {
                        statusCode = HttpStatusCode.Locked,
                        message = string.IsNullOrEmpty(user.LockoutReason) ? "User is locked out" : user.LockoutReason,
                        lockoutExpires = user.LockoutEnd.ToString()
                    });
            
                user.LockoutEnabled = false;
                user.LockoutReason = string.Empty;
                await _dbContext.UpdateUserAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            
            // Check password
            if (user.PasswordHash != StringChiper.GetEncryptedSha256Hash(request.Password, Settings.EncryptionKey))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid password.");

            // Check two-factor authentication
            if (user.TwoFactorEnabled)
            {
                string sessionToken = TokenHelper.GenerateTwoFactorSessionToken();
                string fingerprint = GetMachineFingerprint(user.Id);
                _memoryCacheService.SetValue($"auth:{fingerprint}:tfa-launcher:token", sessionToken, TimeSpan.FromMinutes(5));
                _memoryCacheService.SetValue($"auth:{fingerprint}:tfa-launcher:attempts", 0, TimeSpan.FromMinutes(5));
                    
                return ReturnJson(new
                {
                    statusCode = HttpStatusCode.Redirect,
                    message = "Redirect to 2FA",
                    userId = user.Id,
                    token = sessionToken,
                    url = $"{Settings.ApiUrl}/login/launcher/2fa"
                });
            }

            string host = HttpContext.Request.Host.Host;
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = host,
                Token = TokenHelper.GenerateToken(),
                CreatedAt =  DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
            }, true);
            
            return ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                Message = "Login successful",
                userPlaySession.UserId,
                userPlaySession.Token,
                Expires = userPlaySession.ExpiresAt.ToString(CultureInfo.InvariantCulture)
            });
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Error during login: {Message}", ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }

    /// <summary>
    /// Handles two-factor authentication (2FA) login requests for the launcher.
    /// </summary>
    /// <param name="request">The 2FA session request body containing the session token and 2FA code.</param>
    /// <response code="200">Request successful. Returns user session token and expiration details.</response>
    /// <response code="401">Unauthorized. Invalid session token, session secret, or 2FA code.</response>
    /// <response code="403">Forbidden. Session token expired or too many failed attempts.</response>
    /// <response code="404">Not found. User associated with the session token does not exist.</response>
    /// <response code="423">Locked. User account is locked; includes lockout reason and expiration.</response>
    /// <response code="500">Internal server error. Unexpected error occurred while processing the request.</response>
    [HttpPatch("/login/launcher/2fa")]
    [EnableRateLimiting(RateLimits.AUTH_LOGIN)]
    [Consumes("application/json")]
    [JsonResponse(StatusCodes.Status200OK, typeof(LoginResponse)), TextResponse(StatusCodes.Status401Unauthorized),
     TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LoginLauncherTwoFactorAsync([FromBody] LauncherLoginTFASessionRequestBody request)
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
            
            string fingerprint = GetMachineFingerprint(request.UserId);
            string tokenKey = $"auth:{fingerprint}:tfa-launcher:token";
            if (!_memoryCacheService.TryGetValue(tokenKey, out string? cachedSessionToken) || string.IsNullOrEmpty(cachedSessionToken) || cachedSessionToken != request.SessionToken)
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid or expired session token.");

            string attemptKey = $"auth:{fingerprint}:tfa-launcher:attempts";
            if (!_memoryCacheService.TryGetValue(attemptKey, out int cachedAttempts))
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid or expired session token.");
            
            if (cachedAttempts >= 3)
                return ReturnResponseCode(HttpStatusCode.Forbidden, "To many failed attempts. Please try reauthorizing again.");
            
            CustomUser? user = await _dbContext.FindUserAsync(x => x.Id == request.UserId);
            if (user == null)
                return ReturnResponseCode(HttpStatusCode.NotFound, "User not found.");

            if (user.LockoutEnabled)
            {
                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                    return ReturnJson(new
                    {
                        statusCode = HttpStatusCode.Locked,
                        message = string.IsNullOrEmpty(user.LockoutReason) ? "User is locked out" : user.LockoutReason,
                        lockoutEnd = user.LockoutEnd.ToString()
                    });
            
                user.LockoutEnabled = false;
                user.LockoutReason = string.Empty;
                await _dbContext.UpdateUserAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            
            if (!user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return ReturnResponseCode(HttpStatusCode.Forbidden, "Two-factor authentication is not enabled for this user.");

            var totp = new Totp(Encoding.UTF8.GetBytes(user.TwoFactorSecret));
            if (!totp.VerifyTotp(request.TwoFactorCode, out _, new VerificationWindow(2, 2)))
            {
                _memoryCacheService.SetValue(attemptKey, cachedAttempts + 1);
                return ReturnResponseCode(HttpStatusCode.Unauthorized, "Invalid two-factor code.");
            }
            
            _memoryCacheService.RemoveValue(tokenKey);
            _memoryCacheService.RemoveValue(attemptKey);
            
            string host = HttpContext.Request.Host.Host;
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = host,
                Token = TokenHelper.GenerateToken(),
                CreatedAt =  DateTimeOffset.UtcNow,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(1)
            }, true);
            
            return ReturnJson(new
            {
                statusCode = HttpStatusCode.OK,
                Message = "Login successful",
                userPlaySession.UserId,
                userPlaySession.Token,
                Expires = userPlaySession.ExpiresAt.ToString(CultureInfo.InvariantCulture)
            });
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Error during login: {Message}", ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }


    /// <summary>
    /// Logs out the user by invalidating their session token.
    /// </summary>
    /// <param name="token">
    /// The session token to be invalidated. If not provided, the method attempts to retrieve it 
    /// from the "Authorization" header or the "mmc-token" cookie.
    /// </param>
    /// <response code="200">Logout successful. The user session is terminated.</response>
    /// <response code="400">Bad request. The provided token is invalid or missing.</response>
    /// <response code="500">Internal server error. An unexpected error occurred during logout.</response>
    [HttpPost("/logout")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LogoutAsync(string? token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                if (Request.Headers.TryGetValue("Authorization", out var authHeader))
                    token = authHeader.ToString();
                else if (Request.Cookies.TryGetValue("mmc-token", out var authCookie))
                    token = authCookie;
            }
            
            if (string.IsNullOrEmpty(token))
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid token.");
            
            CustomUserToken? userToken = _dbContext.FindUserToken(x => x.Value == token);
            if (userToken == null)
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid token.");
            
            CustomUserLogin? userLogin = _dbContext.FindUserLogin(x => x.UserId == userToken.UserId && x.ProviderKey == userToken.Id);
            if (userLogin != null)
                await _dbContext.RemoveUserLoginAsync(userLogin);
            
            await _dbContext.RemoveUserTokenAsync(userToken);
            await _dbContext.SaveChangesAsync();
            return SignOut();
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Error during logout: {Message}", ex.Message);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
}