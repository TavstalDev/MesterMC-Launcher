using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database.Interfaces;

namespace Tavstal.MesterMC.Api.Services.Database;

/// <summary>
/// Lightweight composition root that exposes repository instances for all user-related persistence concerns.
/// This class groups the repositories used by higher-level user management services (for example a custom user manager,
/// authentication services, or controllers) so they can be injected and used together.
/// </summary>
public class CustomUserStore
{
    /// <summary>
    /// Repository for the primary user entity (<see cref="CustomUser"/>).
    /// Use this to create, read, update and delete user records.
    /// </summary>
    private IRepository<CustomUser> Users { get; }
    
    /// <summary>
    /// Repository for user claims (<see cref="CustomUserClaim"/>).
    /// Stores claim entries associated with users (claim type/value pairs).
    /// </summary>
    public IRepository<CustomUserClaim> UserClaims { get; }
    
    /// <summary>
    /// Repository for user-role relationships (<see cref="CustomUserRole"/>).
    /// Use to manage which roles are assigned to which users.
    /// </summary>
    public IRepository<CustomUserRole> UserRoles { get; }
    
    /// <summary>
    /// Repository for roles (<see cref="CustomRole"/>).
    /// Contains available roles and their metadata.
    /// </summary>
    public IRepository<CustomRole> Roles { get; }
    
    /// <summary>
    /// Repository for persistent user tokens (<see cref="CustomUserToken"/>).
    /// Commonly used to store refresh tokens, confirmation tokens, or other long-lived tokens.
    /// </summary>
    public IRepository<CustomUserToken> UserTokens { get; }
    
    /// <summary>
    /// Repository for external/user login providers (<see cref="CustomUserLogin"/>).
    /// Stores login provider information (e.g. external OAuth provider keys) linked to users.
    /// </summary>
    public IRepository<CustomUserLogin> UserLogins { get; }
    
    /// <summary>
    /// Repository for role-claim records using the ASP.NET Core identity type <see cref="IdentityRoleClaim{TKey}"/>.
    /// Provides access to claims assigned to roles.
    /// </summary>
    public IRepository<IdentityRoleClaim<string>> RoleClaims { get; }

    /// <summary>
    /// Repository for user backup codes or recovery codes (<see cref="UserBackupCode"/>).
    /// Use when implementing account recovery / two-factor backup code storage.
    /// </summary>
    public IRepository<UserBackupCode> UserBackupCodes { get; }
    
    /// <summary>
    /// Repository for user billing information (<see cref="UserBillingInformation"/>).
    /// Stores payment-related or billing metadata for a user account.
    /// </summary>
    public IRepository<UserBillingInformation> UserBillingInformations { get; }
    
    /// <summary>
    /// Repository for user play sessions (<see cref="UserPlaySession"/>).
    /// Tracks session/activity related information for users (for example for analytics or session history).
    /// </summary>
    public IRepository<UserPlaySession> UserPlaySessions { get; }

    /// <summary>
    /// Constructs a new <see cref="CustomUserStore"/> instance with the specified repositories.
    /// </summary>
    /// <param name="users">Repository for <see cref="CustomUser"/> entities.</param>
    /// <param name="userClaims">Repository for <see cref="CustomUserClaim"/> entities.</param>
    /// <param name="userRoles">Repository for <see cref="CustomUserRole"/> entities.</param>
    /// <param name="roles">Repository for <see cref="CustomRole"/> entities.</param>
    /// <param name="userTokens">Repository for <see cref="CustomUserToken"/> entities.</param>
    /// <param name="userLogins">Repository for <see cref="CustomUserLogin"/> entities.</param>
    /// <param name="roleClaims">Repository for <see cref="IdentityRoleClaim{string}"/> entities.</param>
    /// <param name="userBackupCodes">Repository for <see cref="UserBackupCode"/> entities.</param>
    /// <param name="userBillingInformations">Repository for <see cref="UserBillingInformation"/> entities.</param>
    /// <param name="userPlaySessions">Repository for <see cref="UserPlaySession"/> entities.</param>
    /// <remarks>
    /// All repositories are intended to be provided by the dependency injection container. The store itself is a simple
    /// holder for these dependencies and does not perform additional validation. Consumers should handle null checks if required
    /// or ensure DI provides valid instances.
    /// </remarks>
    public CustomUserStore(
        IRepository<CustomUser> users,
        IRepository<CustomUserClaim> userClaims,
        IRepository<CustomUserRole> userRoles,
        IRepository<CustomRole> roles,
        IRepository<CustomUserToken> userTokens,
        IRepository<CustomUserLogin> userLogins,
        IRepository<IdentityRoleClaim<string>> roleClaims,
        IRepository<UserBackupCode> userBackupCodes,
        IRepository<UserBillingInformation> userBillingInformations,
        IRepository<UserPlaySession> userPlaySessions)
    {
        Users = users;
        UserClaims = userClaims;
        UserRoles = userRoles;
        Roles = roles;
        UserTokens = userTokens;
        UserLogins = userLogins;
        RoleClaims = roleClaims;
        UserBackupCodes = userBackupCodes;
        UserBillingInformations = userBillingInformations;
        UserPlaySessions = userPlaySessions;
    }
    
    public async Task<CustomUser> AddUserAsync(CustomUser value, bool shouldSave = false)
    {
        value.SecurityStamp = Guid.NewGuid().ToString();
        value.ConcurrencyStamp = Guid.NewGuid().ToString();
        return await Users.AddAsync(value, shouldSave);
    }
    
    public async Task<CustomUser> UpdateUserAsync(CustomUser value, bool shouldSave = false)
    {
        value.ConcurrencyStamp = Guid.NewGuid().ToString();
        return await Users.UpdateAsync(value, shouldSave);
    }
    
    public async Task RemoveUserAsync(CustomUser value, bool shouldSave = false)
    {
        await Users.RemoveAsync(value, shouldSave);
    }
    
    public async Task<IEnumerable<CustomUser>> QueryUserAsync(Expression<Func<CustomUser, bool>>? predicate)
    {
        return await Users.QueryAsync(predicate);
    }
    
    public async Task<CustomUser?> FindUserAsync(Expression<Func<CustomUser, bool>>? predicate)
    {
        return await Users.FindAsync(predicate);
    }
    
    public async Task<bool> ExistsUserAsync(Expression<Func<CustomUser, bool>> predicate)
    {
        return await Users.ExistsAsync(predicate);
    }

    public async Task<CustomUser?> FindUserByIdAsync(object id)
    {
        return await Users.FindByIdAsync(id);
    }
}