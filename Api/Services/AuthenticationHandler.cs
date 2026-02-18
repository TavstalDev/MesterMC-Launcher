using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Services;

/// <summary>
/// Custom authentication handler for processing authentication requests.
/// </summary>
public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationHandler"/> class.
    /// </summary>
    /// <param name="options">The options monitor for authentication scheme options.</param>
    /// <param name="logger">The logger factory for creating loggers.</param>
    /// <param name="encoder">The URL encoder for encoding values.</param>
    /// <param name="userManager">The custom user manager for handling user-related operations.</param>
    public AuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        CustomUserManager userManager)
        : base(options, logger, encoder)
    {
        _logger = logger.CreateLogger<AuthenticationHandler>();
        _userManager = userManager;
    }
    
    /// <summary>
    /// Handles the authentication process for incoming requests.
    /// </summary>
    /// <returns>An <see cref="AuthenticateResult"/> indicating the result of the authentication process.</returns>
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            CustomUser? user;
            // Handle authentication by the Authorization header
            if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            {
                string headerValue = authHeader.ToString();
                var authenticationHeader = AuthenticationHeaderValue.Parse(headerValue);
                if (!(authenticationHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase) ||
                      authenticationHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase)))
                    return AuthenticateResult.Fail("Invalid authentication scheme");
                user = await _userManager.GetUserByAuthenticationStringAsync(authenticationHeader.Scheme + " " +
                                                                             authenticationHeader.Parameter);
            }
            else if (Request.Cookies.TryGetValue("mmc-token", out var authCookie))
            {
                user = _userManager.GetUserByCredentials(authCookie);
            }
            else
                return AuthenticateResult.Fail("No authentication information provided in the request");

            // Failed to authenticate the user with the provided authentication information
            if (user == null)
                return AuthenticateResult.Fail("Failed to authenticate user with provided authentication information");

            if (user.LockoutEnabled)
            {
                if (user.LockoutEnd > DateTimeOffset.UtcNow)
                    return AuthenticateResult.Fail(user.LockoutReason ?? "User is locked out");

                user.LockoutEnabled = false;
                await _userManager.UpdateAsync(user);
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email)
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogCritical("An error occurred while authenticating the user. Error: {ErrorMessage}", ex);
            return AuthenticateResult.Fail("An error occurred while authenticating the user");
        }
    }
}