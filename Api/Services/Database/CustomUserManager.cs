using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OtpNet;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Models.Database.User.Claims;
using Tavstal.MesterMC.Api.Utils.Extensions;
using Tavstal.MesterMC.Api.Utils.Helpers;
using KeyGeneration = OtpSharp.KeyGeneration;

namespace Tavstal.MesterMC.Api.Services.Database;

/// <summary>
/// Initializes a new instance of the <see cref="CustomUserManager"/> class.
/// </summary>
public class CustomUserManager(
    IUserStore<CustomUser> store,
    IOptions<IdentityOptions> optionsAccessor,
    IPasswordHasher<CustomUser> passwordHasher,
    IEnumerable<IUserValidator<CustomUser>> userValidators,
    IEnumerable<IPasswordValidator<CustomUser>> passwordValidators,
    ILookupNormalizer keyNormalizer,
    IdentityErrorDescriber errors,
    IServiceProvider services,
    ILogger<CustomUserManager> logger,
    CustomDbContext context,
    IConfiguration configuration,
    MemoryCacheService memoryCacheService,
    Settings settings)
    : UserManager<CustomUser>(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer,
        errors, services, logger)
{
    private readonly TimeSpan CompPassTTL = TimeSpan.FromHours(1);
    

    #region Roles & Claims

    /// <summary>
    /// Defines a list of claim types that should not be exposed.
    /// These claims are used for sensitive operations such as email confirmation, recovery, and two-factor authentication.
    /// </summary>
    public readonly string[] UnexposableClaims =
    [
        CustomClaimTypes.EmailConfirmationToken,
        CustomClaimTypes.EmailRecoveryToken,
        CustomClaimTypes.TwoFactorRecoveryCode,
        CustomClaimTypes.TwoFactorSessionToken,
        CustomClaimTypes.TwoFactorLauncherSessionToken
    ];
    
    #region Roles
    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="roleName">The role name to check.</param>
    /// <returns>True if the user has the role, otherwise false.</returns>
    public bool HasRole(CustomUser user, string roleName)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (user == null)
            return false;

        var role = context.FindRole(x => x.NormalizedName == roleName.Normalize());
        if (role == null)
            return false;

        return context.FindUserRole(x => x.RoleId == role.Id && x.UserId == user.Id) != null;
    }

    /// <summary>
    /// Checks if the user claims principal has a specific role.
    /// </summary>
    /// <param name="userClaims">The user claims principal to check.</param>
    /// <param name="roleName">The role name to check.</param>
    /// <returns>True if the user claims principal has the role, otherwise false.</returns>
    public bool HasRole(ClaimsPrincipal userClaims, string roleName)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (userClaims == null)
            return false;

        if (!userClaims.HasClaim(x => x.Type == "userId"))
            return false;

        var userClaim = userClaims.Claims.FirstOrDefault(x => x.Type == "userId");
        if (userClaim == null)
            return false;
            
        string userid = userClaim.Value;
        CustomUser? user = context.FindUser(x => x.Id == userid);
        if (user == null)
            return false;

        var role = context.FindRole(x => x.NormalizedName == roleName.Normalize());
        if (role == null)
            return false;
        return context.FindUserRole(x => x.RoleId == role.Id && x.UserId == user.Id) != null;
    }
        
    /// <summary>
    /// Checks if the user has a specific role.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user has the role, otherwise false.</returns>
    public bool HasRole(CustomUser user, CustomRole role)
    {
        return context.FindUserRole(x => x.RoleId == role.Id && x.UserId == user.Id) != null;
    }

    /// <summary>
    /// Checks if the user claims principal has a specific role.
    /// </summary>
    /// <param name="userClaims">The user claims principal to check.</param>
    /// <param name="role">The role to check.</param>
    /// <returns>True if the user claims principal has the role, otherwise false.</returns>
    public bool HasRole(ClaimsPrincipal userClaims, CustomRole role)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (userClaims == null)
            return false;

        if (!userClaims.HasClaim(x => x.Type == "userId"))
            return false;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (role == null)
            return false;
            
        var userClaim = userClaims.Claims.FirstOrDefault(x => x.Type == "userId");
        if (userClaim == null)
            return false;

        string userid = userClaim.Value;
        CustomUser? user = context.FindUser(x => x.Id == userid);
        if (user == null)
            return false;

        return context.FindUserRole(x => x.RoleId == role.Id && x.UserId == user.Id) != null;
    }

    /// <summary>
    /// Checks if the user has all specified roles.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="rolenames">The list of role names to check.</param>
    /// <returns>True if the user has all roles, otherwise false.</returns>
    public bool HasAllRole(CustomUser user, List<string> rolenames)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (user == null)
            return false;
            
        foreach (var role in rolenames)
        {
            var r = context.FindRole(x => x.Name.ToLower() == role.ToLower());
            if (r == null)
                return false;
            if (context.FindUserRole(x => x.UserId == user.Id && x.RoleId == r.Id) == null)
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
    public bool HasAnyRole(CustomUser user, List<string> rolenames)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (user == null)
            return false;
            
        foreach (var role in rolenames)
        {
            var r = context.FindRole(x => x.Name.ToLower() == role.ToLower());
            if (r != null)
            {
                if (context.FindUserRole(x => x.UserId == user.Id && x.RoleId == r.Id) != null)
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
    public List<CustomUser> GetUsersByRole(CustomRole role)
    {
        List<CustomUser> users = new List<CustomUser>();
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (role == null)
            return users;

        foreach (var userRole in context.GetUserRoles(x => x.RoleId == role.Id))
        {
            CustomUser? user = context.FindUser(x => x.Id == userRole.UserId);
            if (user == null)
                continue;
                
            if (GetUserCustomRoles(user.Id).Any(x => x.Level > role.Level))
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
    public bool HasHigherRoleThan(CustomUser caller, CustomUser target)
    {
        if (caller.Id == target.Id)
            return true;

        return GetUserHighestRole(caller.Id).Level > GetUserHighestRole(target.Id).Level;
    }
        
    /// <summary>
    /// Gets the custom roles for a user.
    /// </summary>
    /// <param name="userid">The user ID to get the custom roles for.</param>
    /// <returns>A list of custom roles.</returns>
    private List<CustomRole> GetUserCustomRoles(string userid)
    {
        var userRoles = context.GetUserRoles(x => x.UserId == userid);
        List<CustomRole> roles = new List<CustomRole>();
        foreach (var role in userRoles)
        {
            var r = context.FindRole(x => x.Id == role.RoleId);
            if (r != null)
                roles.Add(r);
        }
        return roles;
    }
        
    /// <summary>
    /// Retrieves the roles associated with a specific user.
    /// </summary>
    /// <param name="userid">The ID of the user whose roles are to be retrieved.</param>
    /// <returns>A list of <see cref="CustomRole"/> objects representing the user's roles.</returns>
    public List<CustomRole> GetUserRoles(string userid)
    {
        var userRoles =context. GetUserRoles(x => x.UserId == userid);
        List<CustomRole> roles = new List<CustomRole>();
        foreach (var role in userRoles)
        {
            var r = context.FindRole(x => x.Id == role.RoleId);
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
    public CustomRole GetUserHighestRole(string userid)
    {
        var userRoles =context. GetUserRoles(x => x.UserId == userid);
        List<CustomRole> roles = new List<CustomRole>();
        foreach (var role in userRoles)
        {
            var r = context.FindRole(x => x.Id == role.RoleId);
            if (r != null)
                roles.Add(r);
        }
        return roles.OrderByDescending(x => x.Level).ElementAt(0);
    }
    #endregion
    #region Claims

    /// <summary>
    /// Checks if the user has a specific claim.
    /// </summary>
    /// <param name="userid">The user ID to check.</param>
    /// <param name="claim">The claim to check.</param>
    /// <returns>True if the user has the claim, otherwise false.</returns>
    public bool UserHasClaim(string userid, string claim)
    {
        CustomUser? user = context.FindUser(x => x.Id == userid);
        if (user == null)
            return false;

        return context.FindUserClaim(x => x.UserId == userid && x.ClaimType == claim) != null;
    }

        
    /// <summary>
    /// Checks if the user has a specific claim.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="claim">The claim to check.</param>
    /// <returns>True if the user has the claim, otherwise false.</returns>
    public bool UserHasClaim(CustomUser user, string claim)
    {
        return context.FindUserClaim(x =>  user.Id == x.UserId && x.ClaimType == claim) != null;
    }

    /// <summary>
    /// Checks if the user has a specific claim with a specific value.
    /// </summary>
    /// <param name="userid">The user ID to check.</param>
    /// <param name="claim">The claim to check.</param>
    /// <param name="value">The claim value to check.</param>
    /// <returns>True if the user has the claim with the value, otherwise false.</returns>
    public bool UserHasClaim(string userid, string claim, string value)
    {
        CustomUser? user = context.FindUser(x => x.Id == userid);
        if (user == null)
            return false;

        var localClaim = context.FindUserClaim(x => x.UserId == user.Id && x.ClaimType ==claim);
        if (localClaim == null)
            return false;

        if (localClaim.ClaimValue == null)
            return false;
        return localClaim.ClaimValue.EqualsIgnoreCase(value);
    }

    /// <summary>
    /// Checks if the user has a specific claim with a specific value.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="claim">The claim to check.</param>
    /// <param name="value">The claim value to check.</param>
    /// <returns>True if the user has the claim with the value, otherwise false.</returns>
    public bool UserHasClaim(CustomUser user, string claim, string value)
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (user == null)
            return false;

        var localClaim = context.FindUserClaim(x => x.UserId == user.Id && x.ClaimType == claim);
        if (localClaim == null)
            return false;

        if (string.IsNullOrEmpty(localClaim.ClaimValue))
            return false;
            
        return localClaim.ClaimValue.EqualsIgnoreCase(value);
    }

        
    /// <summary>
    /// Gets the role claims for a user.
    /// </summary>
    /// <param name="userid">The user ID to get the role claims for.</param>
    /// <returns>A list of role claims.</returns>
    private List<IdentityRoleClaim<string>> GetUserRoleClaims(string userid)
    {
        List<IdentityRoleClaim<string>> claims = new List<IdentityRoleClaim<string>>();
        List<CustomRole> roles = GetUserCustomRoles(userid);
        foreach (var role in roles.OrderByDescending(x => x.Level))
        {
            claims.AddRange(context.GetRoleClaims(x => x.RoleId == role.Id));
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
    public async Task SetUserClaim(CustomUser user, string claim, string value)
    {
        var localClaim = context.FindUserClaim(x => x.UserId == user.Id && x.ClaimType == claim);
        if (localClaim == null)
        {
            await context.AddUserClaimAsync(new CustomUserClaim
            {
                UserId = user.Id,
                ClaimType = claim,
                ClaimValue = value
            }, true);
            return;
        }

        localClaim.ClaimValue = value;
        await context.UpdateUserClaimAsync(localClaim, true);
    }
    #endregion
        
    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="userid">The user ID to check.</param>
    /// <param name="claim">The claim to check.</param>
    /// <param name="value">The claim value to check.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    public bool HasPermission(string userid, string claim, string value = "true")
    {
        var roleClaim = context.FindUserClaim(x => x.UserId == userid && x.ClaimType == claim && x.ClaimValue == value);
        if (roleClaim != null)
            return true;

        return GetUserRoleClaims(userid).Any(x => x.ClaimType == claim && x.ClaimValue == value);
    }

    /// <summary>
    /// Checks if the user has a specific permission.
    /// </summary>
    /// <param name="user">The user to check.</param>
    /// <param name="claim">The claim to check.</param>
    /// <param name="value">The claim value to check.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    public bool HasPermission(CustomUser user, string claim, string value = "true")
    {
        return HasPermission(user.Id, claim, value);
    }

    /// <summary>
    /// Gets the users with a specific permission.
    /// </summary>
    /// <param name="claim">The claim to check.</param>
    /// <param name="value">The claim value to check.</param>
    /// <returns>A list of users with the permission.</returns>
    public List<CustomUser> GetUsersByPermission(string claim, string value = "true")
    {
        List<CustomUser> users = new List<CustomUser>();
        foreach (var user in context.GetUsers())
            if (HasPermission(user, claim, value))
                users.Add(user);

        return users;
    }
        
    /// <summary>
    /// Retrieves all claims associated with a user, including role claims if specified.
    /// </summary>
    /// <param name="userId">The ID of the user whose claims are to be retrieved.</param>
    /// <param name="includeRoles">A boolean indicating whether to include role claims (default is true).</param>
    /// <returns>A list of <see cref="CustomClaim"/> objects representing the user's claims.</returns>
    public List<CustomClaim> GetAllClaimsOfUser(string userId, bool includeRoles = true)
    {
        List<CustomClaim> claims = new List<CustomClaim>();
        List<string> addedKeys = new List<string>();
        List<CustomUserClaim> userClaims = context.GetUserClaims(x => x.UserId == userId);
        foreach (var userClaim in userClaims)
        {
            if (string.IsNullOrEmpty(userClaim.ClaimType))
                continue;
            
            if (UnexposableClaims.Contains(userClaim.ClaimType))
                continue;
                
            if (addedKeys.Contains(userClaim.ClaimType))
                continue;
                
            addedKeys.Add(userClaim.ClaimType);
            claims.Add(new CustomClaim(userClaim.ClaimType, userClaim.ClaimValue ?? string.Empty));
        }
            
        if (includeRoles)
            foreach (var userClaim in GetUserRoleClaims(userId))
            {
                if (string.IsNullOrEmpty(userClaim.ClaimType))
                    continue;
                
                if (UnexposableClaims.Contains(userClaim.ClaimType))
                    continue;
                    
                if (addedKeys.Contains(userClaim.ClaimType))
                    continue;
                
                addedKeys.Add(userClaim.ClaimType);
                claims.Add(new CustomClaim(userClaim.ClaimType, userClaim.ClaimValue ?? string.Empty));
            }

        // ReSharper disable once RedundantAssignment
        addedKeys = new  List<string>();
        return claims;
    }
    
    /// <summary>
    /// Retrieves all claims associated with a user, including role claims if specified.
    /// </summary>
    /// <param name="userId">The ID of the user whose claims are to be retrieved.</param>
    /// <param name="includeRoles">A boolean indicating whether to include role claims (default is true).</param>
    /// <returns>A list of <see cref="Claim"/> objects representing the user's claims.</returns>
    public List<Claim> GetAllClaimsOfUserV2(string userId, bool includeRoles = true)
    {
        List<Claim> claims = new List<Claim>();
        List<string> addedKeys = new List<string>();
        List<CustomUserClaim> userClaims = context.GetUserClaims(x => x.UserId == userId);
        foreach (var userClaim in userClaims)
        {
            if (string.IsNullOrEmpty(userClaim.ClaimType))
                continue;
                
            if (addedKeys.Contains(userClaim.ClaimType))
                continue;
                
            addedKeys.Add(userClaim.ClaimType);
            claims.Add(new Claim(userClaim.ClaimType, userClaim.ClaimValue ?? string.Empty));
        }
            
        if (includeRoles)
            foreach (var userClaim in GetUserRoleClaims(userId))
            {
                if (string.IsNullOrEmpty(userClaim.ClaimType))
                    continue;
                    
                if (addedKeys.Contains(userClaim.ClaimType))
                    continue;
                
                addedKeys.Add(userClaim.ClaimType);
                claims.Add(new Claim(userClaim.ClaimType, userClaim.ClaimValue ?? string.Empty));
            }

        // ReSharper disable once RedundantAssignment
        addedKeys = new  List<string>();
        return claims;
    }
    #endregion
    
    /// <summary>
    /// Retrieves a user based on their credentials token.
    /// </summary>
    /// <param name="token">The token used to identify the user.</param>
    /// <returns>
    /// The <see cref="CustomUser"/> object if the token is valid and associated with a user; otherwise, null.
    /// </returns>
    public CustomUser? GetUserByCredentials(string token)
    {
        var userToken = context.FindUserToken(x => x.Value == token);
        if (userToken == null)
            return null;

        var userLogin = context.FindUserLogin(x => x.UserId == userToken.UserId && x.ProviderKey == userToken.Id && x.ExpireDate > DateTimeOffset.UtcNow);
        if (userLogin == null)
            return null;
            
        return context.FindUser(x => x.Id == userToken.UserId);
    }
    
    /// <summary>
    /// Retrieves a user based on an authentication string.
    /// </summary>
    /// <param name="authenticationString">The authentication string (e.g., Basic or Bearer token).</param>
    /// <returns>
    /// The <see cref="CustomUser"/> object if the authentication string is valid and associated with a user; otherwise, null.
    /// </returns>
    public async Task<CustomUser?> GetUserByAuthenticationStringAsync(string? authenticationString)
    {
        if (string.IsNullOrEmpty(authenticationString))
            return null;
            
        string lowerAuthString = authenticationString.ToLower();
        if (lowerAuthString.StartsWith("basic"))
        {
            string value = authenticationString.Remove(0, 6);
            if (value.EndsWith("=") || !(value.Contains(":") || value.Contains(".")))
                value = Encoding.UTF8.GetString(Convert.FromBase64String(value));
            
            var raw = value.Split(value.Contains(':') ? ":" : ".");
            if (raw.Length < 2)
                return null;

            string normalizedValue = raw[0].Normalize().ToUpper();
            return await context.FindUserAsync(x => (x.NormalizedEmail == normalizedValue || x.NormalizedUserName == normalizedValue) && x.PasswordHash == StringChiper.GetEncryptedSha256Hash(raw[1], settings.EncryptionKey));
        }

        if (lowerAuthString.StartsWith("bearer"))
        {
            string token = authenticationString.Remove(0, 7);
            var userToken = context.FindUserToken(x => x.Value == token);
            if (userToken == null)
                return null;

            var userLogin = context.FindUserLogin(x => x.UserId == userToken.UserId && x.ProviderKey == userToken.Id && x.ExpireDate > DateTimeOffset.UtcNow);
            if (userLogin == null)
                return null;

            return await context.FindUserAsync(x => x.Id == userToken.UserId);
        }

        return null;
    }
    
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
            using var client = new HttpClient();
            response = await client.GetStringAsync($"https://api.pwnedpasswords.com/range/{prefix}");
            if (!string.IsNullOrEmpty(cacheKey))
                memoryCacheService.SetValue(cacheKey, response, CompPassTTL);
        }
        
        if (string.IsNullOrEmpty(response))
            return false;
        
        return response.Contains(suffix);
    }

    /// <summary>
    /// Generates a secret key for two-factor authentication.
    /// </summary>
    /// <returns>A base32-encoded string representing the secret key.</returns>
    public string GenerateSecretKey()
    {
        var key = KeyGeneration.GenerateRandomKey(20);
        return Base32Encoding.ToString(key);
    }
}