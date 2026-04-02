using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Utils.Helpers;

#pragma warning disable CS9113 // Parameter is unread.

namespace Tavstal.MesterMC.Api.Services.Database;

/// <summary>
/// Initializes a new instance of the <see cref="CustomUserManager"/> class.
/// </summary>
public class CustomUserManager(
    CustomUserStore userStore,
    IPasswordHasher<CustomUser> passwordHasher,
    IHttpClientFactory httpClientFactory,
    ILogger<CustomUserManager> logger,
    CustomDbContext context,
    MemoryCacheService memoryCacheService,
    Settings settings)
{
    private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler = new();
    private readonly TokenValidationParameters _tokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidIssuer = settings.Issuer,
        
        ValidateAudience = true,
        ValidAudience = settings.Audience,
        
        ValidateLifetime = true,
        ClockSkew = settings.ClockSkew,
        
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.EncryptionKey)),
    };
    private readonly TimeSpan CompPassTTL = TimeSpan.FromHours(1);
    

    #region Roles & Claims
    
    #region Roles
    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="roleName">The role name to check.</param>
    /// <returns>True if the user has the role, otherwise false.</returns>
    public async Task<bool> HasRoleAsync(CustomUser user, string roleName)
    {
        var role = await userStore.Roles.FindAsync(x => x.NormalizedName == roleName.Normalize());
        if (role == null)
            return false;
        return await userStore.UserRoles.ExistsAsync(x => x.RoleId == role.Id && x.UserId == user.Id);
    }

    /// <summary>
    /// Checks if the user claims principal has a specific role.
    /// </summary>
    /// <param name="userClaims">The user claims principal to check.</param>
    /// <param name="roleName">The role name to check.</param>
    /// <returns>True if the user claims principal has the role, otherwise false.</returns>
    public async Task<bool> HasRoleAsync(ClaimsPrincipal userClaims, string roleName)
    {
        if (!userClaims.HasClaim(x => x.Type == "userId"))
            return false;

        var userClaim = userClaims.Claims.FirstOrDefault(x => x.Type == "userId");
        if (userClaim == null)
            return false;
            
        string userid = userClaim.Value;
        CustomUser? user = await userStore.FindUserByIdAsync(userid);
        if (user == null)
            return false;

        var role = await userStore.Roles.FindAsync(x => x.NormalizedName == roleName.Normalize());
        if (role == null)
            return false;
        return await userStore.UserRoles.ExistsAsync(x => x.RoleId == role.Id && x.UserId == user.Id);
    }
        
    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role, otherwise false.</returns>
    public async Task<bool> HasRoleAsync(CustomUser user, CustomRole role)
    {
        return await userStore.UserRoles.ExistsAsync(x => x.RoleId == role.Id && x.UserId == user.Id);
    }

    /// <summary>
    /// Checks if the user claims principal has a specific role.
    /// </summary>
    /// <param name="userClaims">The user claims principal to check.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user claims principal has the role, otherwise false.</returns>
    public async Task<bool> HasRoleAsync(ClaimsPrincipal userClaims, CustomRole role)
    {
        if (!userClaims.HasClaim(x => x.Type == "userId"))
            return false;
            
        var userClaim = userClaims.Claims.FirstOrDefault(x => x.Type == "userId");
        if (userClaim == null)
            return false;

        string userid = userClaim.Value;
        CustomUser? user = await userStore.FindUserByIdAsync(userid);
        if (user == null)
            return false;

        return await userStore.UserRoles.ExistsAsync(x => x.RoleId == role.Id && x.UserId == user.Id);
    }

    /// <summary>
    /// Checks if the user has all specified roles.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="rolenames">The list of role names to check.</param>
    /// <returns>True if the user has all roles, otherwise false.</returns>
    public async Task<bool> HasAllRoleAsync(CustomUser user, List<string> rolenames)
    {
        foreach (var role in rolenames)
        {
            var r = await userStore.Roles.FindAsync(x => x.Name.ToLower() == role.ToLower());
            if (r == null)
                return false;
            if (!await userStore.UserRoles.ExistsAsync(x => x.UserId == user.Id && x.RoleId == r.Id))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the user has any of the specified roles.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="rolenames">The list of role names to check.</param>
    /// <returns>True if the user has any role, otherwise false.</returns>
    public async Task<bool> HasAnyRoleAsync(CustomUser user, List<string> rolenames)
    {
        foreach (var role in rolenames)
        {
            var r = await userStore.Roles.FindAsync(x => x.Name.ToLower() == role.ToLower());
            if (r != null)
            {
                if (await userStore.UserRoles.ExistsAsync(x => x.UserId == user.Id && x.RoleId == r.Id))
                    return true;
            }
        }

        return false;
    }
        
    /// <summary>
    /// Gets the users with a specific role.
    /// </summary>
    /// <param name="role">The role to check.</param>
    /// <returns>A list of users with the role.</returns>
    public async Task<List<CustomUser>> GetUsersByRoleAsync(CustomRole role)
    {
        List<CustomUser> users = [];
        foreach (var userRole in await userStore.UserRoles.QueryAsync(x => x.RoleId == role.Id))
        {
            CustomUser? user = await userStore.FindUserByIdAsync(userRole.UserId);
            if (user == null)
                continue;
                
            if ((await GetUserCustomRolesAsync(user.Id)).Any(x => x.Level > role.Level))
                continue;

            users.Add(user);
        }

        return users;
    }

        
    /// <summary>
    /// Checks if the caller has a higher role than the target.
    /// </summary>
    /// <param name="caller">The caller to check.</param>
    /// <param name="target">The target to check.</param>
    /// <returns>True if the caller has a higher role, otherwise false.</returns>
    public async Task<bool> HasHigherRoleThanAsync(CustomUser caller, CustomUser target)
    {
        if (caller.Id == target.Id)
            return true;

        return (await GetUserHighestRoleAsync(caller.Id)).Level > (await GetUserHighestRoleAsync(target.Id)).Level;
    }
        
    /// <summary>
    /// Gets the custom roles for a user.
    /// </summary>
    /// <param name="userid">The user ID to get the custom roles for.</param>
    /// <returns>A list of custom roles.</returns>
    private async Task<List<CustomRole>> GetUserCustomRolesAsync(string userid)
    {
        var userRoles = await userStore.UserRoles.QueryAsync(x => x.UserId == userid);
        List<CustomRole> roles = [];
        foreach (var role in userRoles)
        {
            var r = await userStore.Roles.FindByIdAsync(role.RoleId);
            if (r != null)
                roles.Add(r);
        }
        return roles;
    }

    /// <summary>
    /// Gets the highest role for a user.
    /// </summary>
    /// <param name="userid">The user ID to get the highest role for.</param>
    /// <returns>The highest role.</returns>
    public async Task<CustomRole> GetUserHighestRoleAsync(string userid)
    {
        var userRoles = await userStore.UserRoles.QueryAsync(x => x.UserId == userid);
        List<CustomRole> roles = [];
        foreach (var role in userRoles)
        {
            var r = await userStore.Roles.FindByIdAsync(role.RoleId);
            if (r != null)
                roles.Add(r);
        }
        return roles.OrderByDescending(x => x.Level).ElementAt(0);
    }
    #endregion
    #region Claims
    /// <summary>
    /// Gets the role claims for a user.
    /// </summary>
    /// <param name="userid">The user ID to get the role claims for.</param>
    /// <returns>A list of role claims.</returns>
    private async Task<List<IdentityRoleClaim<string>>> GetUserRoleClaimsAsync(string userid)
    {
        List<IdentityRoleClaim<string>> claims = [];
        List<CustomRole> roles = await GetUserCustomRolesAsync(userid);
        foreach (var role in roles.OrderByDescending(x => x.Level))
        {
            claims.AddRange(await  userStore.RoleClaims.QueryAsync(x => x.RoleId == role.Id));
        }
        return claims;
    }

    /// <summary>
    /// Sets a user claim.
    /// </summary>
    /// <param name="user">The user to set the claim for.</param>
    /// <param name="claim">The claim to set.</param>
    /// <param name="value">The claim value to set.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task SetUserClaimAsync(CustomUser user, string claim, string value)
    {
        var localClaim = await userStore.UserClaims.FindAsync(x => x.UserId == user.Id && x.ClaimType == claim);
        if (localClaim == null)
        {
            await userStore.UserClaims.AddAsync(new CustomUserClaim
            {
                UserId = user.Id,
                ClaimType = claim,
                ClaimValue = value
            }, true);
            return;
        }

        localClaim.ClaimValue = value;
        await userStore.UserClaims.UpdateAsync(localClaim, true);
    }
    #endregion
        
    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="userid">The user ID to check.</param>
    /// <param name="claim">The claim to check.</param>
    /// <param name="value">The claim value to check.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    public async Task<bool> HasPermissionAsync(string userid, string claim, string value = "true")
    {
        var roleClaim = await userStore.UserClaims.FindAsync(x => x.UserId == userid && x.ClaimType == claim && x.ClaimValue == value);
        if (roleClaim != null)
            return true;
        
        List<CustomRole> roles = await GetUserCustomRolesAsync(userid);
        foreach (var role in roles.OrderByDescending(x => x.Level))
        {
            if (await userStore.RoleClaims.ExistsAsync(x => x.RoleId == role.Id && x.ClaimType == claim && x.ClaimValue == value))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="claim">The claim to check.</param>
    /// <param name="value">The claim value to check.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    public async Task<bool> HasPermissionAsync(CustomUser user, string claim, string value = "true")
    {
        return await HasPermissionAsync(user.Id, claim, value);
    }
    #endregion

    #region Authentication
    /// <summary>
    /// Checks if a password has been compromised using the Pwned Passwords API.
    /// </summary>
    /// <param name="password">The password to check.</param>
    /// <returns>
    /// A boolean value indicating whether the password has been compromised (true) or not (false).
    /// </returns>
    public async Task<bool> IsCompromisedPasswordAsync(string password)
    {
        using var sha1 = SHA1.Create();
        byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
        string hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

        string prefix = hashString.Substring(0, 5);
        string suffix = hashString.Substring(5);

        string cacheKey = $"pwned:{prefix}";
        if (!memoryCacheService.TryGetValue(cacheKey, out string? response))
        {
            using var client = httpClientFactory.CreateClient();
            response = await client.GetStringAsync($"https://api.pwnedpasswords.com/range/{prefix}");
            if (!string.IsNullOrEmpty(cacheKey))
                memoryCacheService.SetValue(cacheKey, response, CompPassTTL);
        }
        
        return !string.IsNullOrEmpty(response) && response.Contains(suffix);
    }

    public string CreateJwtToken(TimeSpan duration, ClaimsIdentity? claims = null)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateJwtSecurityToken(
            settings.Issuer, 
            settings.Audience, 
            claims, 
            DateTime.UtcNow, 
            DateTime.UtcNow.Add(duration), 
            DateTime.UtcNow,
            new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.EncryptionKey)),
                SecurityAlgorithms.HmacSha256)
            );
        return handler.WriteToken(token);
    }

    public async Task<CustomUser?> VerifyPasswordAsync(string username, string password)
    {
        string normalizedUsername = username.Normalize().ToUpper();
        CustomUser? user = await userStore.FindUserAsync(x => x.NormalizedEmail == normalizedUsername || x.NormalizedUserName == normalizedUsername);
        if (user == null)
            return null;
        
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        switch (result)
        {
            case PasswordVerificationResult.Failed:
                user.AccessFailedCount++;
                if (user.AccessFailedCount > settings.LockoutMaxAttempts)
                {
                    user.LockoutEnabled = true;
                    user.LockoutEnd = DateTimeOffset.UtcNow.Add(settings.LockoutDuration);
                    user.LockoutReason = "Too many failed login attempts.";
                }
                await userStore.UpdateUserAsync(user, true);
                return null;
            case PasswordVerificationResult.SuccessRehashNeeded:
                user.PasswordHash = passwordHasher.HashPassword(user, password);
                user.SecurityStamp = Guid.NewGuid().ToString();
                await userStore.UpdateUserAsync(user, true);
                break;
        }
        
        return user;
    }

    public async Task<CustomUser?> VerifyTokenLoginAsync(string token)
    {
        if (!await VerifyJwtTokenAsync(token))
            return null;
        
        CustomUserToken? userToken = await userStore.UserTokens.FindAsync(x => x.Value == token);
        if (userToken == null)
            return null;

        CustomUserLogin? userLogin = await userStore.UserLogins.FindAsync(x => x.UserId == userToken.UserId && x.ProviderKey == userToken.Id && x.ExpireDate > DateTimeOffset.UtcNow);
        if (userLogin == null)
            return null;

        return await userStore.FindUserByIdAsync(userToken.UserId);
    }
    
    public async Task<bool> VerifyJwtTokenAsync(string token)
    {
        try
        {
            var result = await _jwtSecurityTokenHandler.ValidateTokenAsync(token, _tokenValidationParameters);
            if (result == null || !result.IsValid)
                return false;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while verifying the token.");
            return false;
        }
    }
    
    public string GetMachineFingerprint(HttpRequest httpRequest, string userId)
    {
        var userAgent = httpRequest.Headers.UserAgent.ToString();
        var ipAddress = httpRequest.HttpContext.Connection.RemoteIpAddress?.ToString();
    
        // Combine traits and hash them
        var rawData = $"{userId}-{userAgent}-{ipAddress}";
        return StringChiper.GetEncryptedHash(rawData, settings.EncryptionKey);
    }
    #endregion

    #region TwoFactor Authentication

    public async Task<string> GenerateTwoFactorTokenAsync(CustomUser user)
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        string token = Encoding.UTF8.GetString(key);
        user.TwoFactorSecret = token.EncryptSelf(settings.EncryptionKey);
        user.SecurityStamp  = Guid.NewGuid().ToString();
        await userStore.UpdateUserAsync(user, true);
        return token;
    }
    
    public bool VerifyTwoFactorCode(CustomUser user, string code)
    {
        if (string.IsNullOrEmpty(user.TwoFactorSecret))
            return false;
        
        string decryptedSecret = user.TwoFactorSecret.DecryptSelf(settings.EncryptionKey);
        var totp = new Totp(Encoding.UTF8.GetBytes(decryptedSecret));
        return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
    }
    
    public async Task<IEnumerable<string>?> GenerateNewTwoFactorRecoveryCodesAsync(CustomUser user, int number)
    {
        var existingCodes = await userStore.UserBackupCodes.QueryAsync(x => x.UserId == user.Id);
        foreach (var code in existingCodes)
            await userStore.UserBackupCodes.RemoveAsync(code);
        await context.SaveChangesAsync();
        List<string> newCodes = [];
        for (int i = 0; i < number; i++)
        {
            string code = TokenHelper.GenerateRandomString(length: 10);
            newCodes.Add(code);
            await userStore.UserBackupCodes.AddAsync(new UserBackupCode
            {
                UserId = user.Id,
                HashedCode = StringChiper.GetEncryptedHash(code, settings.EncryptionKey),
                CreateAt = DateTime.UtcNow
            });
        }
        await context.SaveChangesAsync();
        return newCodes;
    }
    #endregion
}