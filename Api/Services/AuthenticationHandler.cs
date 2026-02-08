using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Services;

public class AuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly CustomUserManager _userManager;
    
    public AuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        CustomUserManager userManager)
        : base(options, logger, encoder)
    {
        _userManager = userManager;
    }
    
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey("Authorization"))
            return AuthenticateResult.Fail("Authorization header is missing");

        string? authenticationHeaderValue = Request.Headers["Authorization"];
        if (authenticationHeaderValue == null)
            return AuthenticateResult.Fail("Authorization header is incorrect");
        var authenticationHeader = AuthenticationHeaderValue.Parse(authenticationHeaderValue);

        if (!(authenticationHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase) || authenticationHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase)))
            return AuthenticateResult.Fail("Invalid authentication scheme");

        CustomUser? user = await _userManager.GetUserByAuthenticationStringAsync(authenticationHeader.Scheme + " " + authenticationHeader.Parameter);
        if (user == null)
        {
            if (authenticationHeader.Scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                return AuthenticateResult.Fail("Invalid Bearer token.");
            return AuthenticateResult.Fail("Invalid username or password");
        }

        if (user.LockoutEnabled)
        {
            if (user.LockoutEnd > DateTime.Now)
                return AuthenticateResult.Fail(user.LockoutReason ?? "User is locked out");
            
            user.LockoutEnabled = false;
            await _userManager.UpdateAsync(user);
        }
            
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email!)
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}