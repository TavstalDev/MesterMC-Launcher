using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.Server;
using Tavstal.MesterMC.Api.Models.Database.User;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Tavstal.MesterMC.Api.Services.Database;

/// <summary>
/// Custom database context for the application, extending IdentityDbContext with custom user and role entities.
/// </summary>
public class CustomDbContext : IdentityDbContext<CustomUser, CustomRole, string, CustomUserClaim, CustomUserRole,
    CustomUserLogin, IdentityRoleClaim<string>, CustomUserToken>
{
    private readonly ILogger<CustomDbContext> _logger;

    /// <remarks>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE THE CUSTOM METHODS IN THIS CONTEXT.
    /// </remarks>
    public new DbSet<CustomUser> Users { get; private set; }

    /// <remarks>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE THE CUSTOM METHODS IN THIS CONTEXT.
    /// </remarks>
    public new DbSet<CustomUserClaim> UserClaims { get; private set; }

    /// <remarks>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE THE CUSTOM METHODS IN THIS CONTEXT.
    /// </remarks>
    public new DbSet<CustomUserRole> UserRoles { get; private set; }

    /// <remarks>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE THE CUSTOM METHODS IN THIS CONTEXT.
    /// </remarks>
    public new DbSet<CustomRole> Roles { get; private set; }

    /// <remarks>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE THE CUSTOM METHODS IN THIS CONTEXT.
    /// </remarks>
    public new DbSet<CustomUserToken> UserTokens { get; private set; }

    /// <remarks>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE THE CUSTOM METHODS IN THIS CONTEXT.
    /// </remarks>
    public new DbSet<CustomUserLogin> UserLogins { get; private set; }

    /// <remarks>
    /// DO NOT USE THIS PROPERTY DIRECTLY, USE THE CUSTOM METHODS IN THIS CONTEXT.
    /// </remarks>
    public new DbSet<IdentityRoleClaim<string>> RoleClaims { get; private set; }

    private DbSet<UserBillingInformation> UserBillingInformations { get; set; }
    
    private DbSet<UserPlaySession> UserPlaySessions { get; set; }
    
    private DbSet<FileData> Files { get; set; }

    private DbSet<Cape> Capes { get; set; }
    
    private DbSet<UserCape> UserCapes { get; set; }
    
    private DbSet<ServerJoin> ServerJoins { get; set; }
    
    private DbSet<News> News { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
    /// <param name="logger">The logger instance.</param>
    public CustomDbContext(DbContextOptions<CustomDbContext> options, ILogger<CustomDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    /// <summary>
    /// Configures the schema needed for the identity framework.
    /// </summary>
    /// <param name="builder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<CustomUserClaim>()
            .HasOne(c => c.User) // Navigation property in IdentityUserClaim
            .WithMany(u => u.Claims) // Navigation property in CustomUser
            .HasForeignKey(c => c.UserId) // Foreign key
            .IsRequired(); // Ensure the relationship is required

        builder.Entity<CustomUserToken>()
            .HasOne(t => t.User) // Navigation property in CustomUserToken
            .WithMany(u => u.Tokens) // Navigation property in CustomUser
            .HasForeignKey(t => t.UserId) // Foreign key to CustomUser
            .IsRequired(); // Ensure the relationship is required

        builder.Entity<CustomUserToken>()
            .HasKey(x => x.Id);

        builder.Entity<CustomUserToken>()
            .Property(x => x.Value)
            .HasDefaultValueSql("UUID()")
            .ValueGeneratedOnAdd();

        builder.Entity<CustomUserRole>()
            .HasOne(c => c.User) // Navigation property in IdentityUserClaim
            .WithMany(u => u.UserRoles) // Navigation property in CustomUser
            .HasForeignKey(c => c.UserId) // Foreign key
            .IsRequired(); // Ensure the relationship is requir

        // Ensure that the foreign key is correctly mapped
        builder.Entity<CustomUserLogin>(entity =>
        {
            entity.HasKey(cul => cul.Id);
            entity.Property(cul => cul.Id)
                .ValueGeneratedOnAdd();

            entity.HasOne(cul => cul.User)
                .WithMany(cu => cu.UserLogins)
                .HasForeignKey(cul => cul.UserId)
                .IsRequired();
            
            entity.ToTable("AspNetUserLogins"); 
        });

        builder.Entity<UserBillingInformation>()
            .HasOne(b => b.User)
            .WithOne(u => u.BillingInformation)
            .HasForeignKey<UserBillingInformation>(b => b.UserId)
            .IsRequired();
        
        builder.Entity<UserPlaySession>()
            .HasOne(p => p.User)
            .WithMany(u => u.PlaySessions) 
            .HasForeignKey(p => p.UserId) 
            .IsRequired();
        
        builder.Entity<FileData>()
            .HasOne(f => f.User)
            .WithMany(u => u.Files)
            .HasForeignKey(f => f.UserId)
            .IsRequired(false);

        builder.Entity<UserCape>()
            .HasOne(c => c.User)
            .WithMany(u => u.OwnedCapes)
            .HasForeignKey(c => c.UserId)
            .IsRequired();
    }


    /// <summary>
    /// Configures the database (and other options) to be used for this context.
    /// </summary>
    /// <param name="optionsBuilder">A builder used to create or modify options for this context.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;
        CustomDbInitializer.Initialize(this);
    }

    /// <summary>
    /// Discards all changes made to the tracked entities.
    /// </summary>
    public void DiscardChanges()
    {
        ChangeTracker.Clear();
    }

    #region User

    #region User Data

    /// <summary>
    /// Adds a new user to the database asynchronously.
    /// </summary>
    /// <param name="value">The user to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the user.</param>
    /// <returns>The added user.</returns>
    public async Task<CustomUser> AddUserAsync(CustomUser value, bool shouldSave = false)
    {
        var result = await Users.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Updates an existing user in the database asynchronously.
    /// </summary>
    /// <param name="value">The user to update.</param>
    /// <param name="shouldSave">Whether to save changes after updating the user.</param>
    public async Task UpdateUserAsync(CustomUser value, bool shouldSave = false)
    {
        Users.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Removes an existing user from the database asynchronously.
    /// </summary>
    /// <param name="value">The user to remove.</param>
    /// <param name="shouldSave">Whether to save changes after removing the user.</param>
    public async Task RemoveUserAsync(CustomUser value, bool shouldSave = false)
    {
        Users.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Gets a list of users from the database.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the users.</param>
    /// <returns>A list of users.</returns>
    public List<CustomUser> GetUsers(Expression<Func<CustomUser, bool>>? predicate = null)
    {
        if (predicate != null)
            return Users.Where(predicate).ToList();
        return Users.ToList();
    }

    /// <summary>
    /// Finds a user in the database based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the user.</param>
    /// <returns>The found user, or null if no user is found.</returns>
    public CustomUser? FindUser(Expression<Func<CustomUser, bool>>? predicate)
    {
        if (predicate != null)
            return Users.Find(predicate);
        return Users.FirstOrDefault();
    }

    /// <summary>
    /// Finds a user in the database asynchronously based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the user.</param>
    /// <returns>The found user, or null if no user is found.</returns>
    public async Task<CustomUser?> FindUserAsync(Expression<Func<CustomUser, bool>> predicate)
    {
        return await Users.FirstOrDefaultAsync(predicate);
    }

    #endregion

    #region User Claims

    /// <summary>
    /// Adds or updates a user claim in the database asynchronously.
    /// </summary>
    /// <param name="value">The user claim to add or update.</param>
    /// <param name="shouldSave">Indicates whether to save changes to the database after the operation.</param>
    /// <returns>
    /// The added or updated IdentityUserClaim entity, or null if the operation fails.
    /// </returns>
    public async Task<CustomUserClaim?> SetUserClaimAsync(CustomUserClaim value, bool shouldSave = false)
    {
        EntityEntry<CustomUserClaim> result;
        var claim = await UserClaims.FirstOrDefaultAsync(x =>
            x.UserId == value.UserId && x.ClaimType == value.ClaimType);
        if (claim == null)
            result = await UserClaims.AddAsync(value);
        else
            result = UserClaims.Update(value);

        if (shouldSave)
            await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Adds a new user claim to the database asynchronously.
    /// </summary>
    /// <param name="value">The user claim to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the user claim.</param>
    /// <returns>The added user claim.</returns>
    public async Task<CustomUserClaim> AddUserClaimAsync(CustomUserClaim value, bool shouldSave = false)
    {
        var result = await UserClaims.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Updates an existing user claim in the database asynchronously.
    /// </summary>
    /// <param name="value">The user claim to update.</param>
    /// <param name="shouldSave">Whether to save changes after updating the user claim.</param>
    public async Task UpdateUserClaimAsync(CustomUserClaim value, bool shouldSave = false)
    {
        UserClaims.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }


    /// <summary>
    /// Removes an existing user claim from the database asynchronously.
    /// </summary>
    /// <param name="value">The user claim to remove.</param>
    /// <param name="shouldSave">Whether to save changes after removing the user claim.</param>
    public async Task RemoveUserClaimAsync(CustomUserClaim value, bool shouldSave = false)
    {
        UserClaims.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Gets a list of user claims from the database.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the user claims.</param>
    /// <returns>A list of user claims.</returns>
    public List<CustomUserClaim> GetUserClaims(Expression<Func<CustomUserClaim, bool>>? predicate = null)
    {
        if (predicate != null)
            return UserClaims.Where(predicate).ToList();
        return UserClaims.ToList();
    }

    /// <summary>
    /// Finds a user claim in the database based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the user claim.</param>
    /// <returns>The found user claim, or null if no user claim is found.</returns>
    public CustomUserClaim? FindUserClaim(Expression<Func<CustomUserClaim, bool>> predicate)
    {
        return UserClaims.FirstOrDefault(predicate);
    }

    #endregion

    #region Roles

    /// <summary>
    /// Adds a new role to the database asynchronously.
    /// </summary>
    /// <param name="value">The role to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the role.</param>
    /// <returns>The added role.</returns>
    public async Task<CustomRole> AddRoleAsync(CustomRole value, bool shouldSave = false)
    {
        var result = await Roles.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Adds multiple roles to the database asynchronously.
    /// </summary>
    /// <param name="value">The list of roles to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the roles.</param>
    public async Task AddRolesAsync(List<CustomRole> value, bool shouldSave = false)
    {
        await Roles.AddRangeAsync(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing role in the database asynchronously.
    /// </summary>
    /// <param name="value">The role to update.</param>
    /// <param name="shouldSave">Whether to save changes after updating the role.</param>
    public async Task UpdateRoleAsync(CustomRole value, bool shouldSave = false)
    {
        Roles.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Removes an existing role from the database asynchronously.
    /// </summary>
    /// <param name="value">The role to remove.</param>
    /// <param name="shouldSave">Whether to save changes after removing the role.</param>
    public async Task RemoveRoleAsync(CustomRole value, bool shouldSave = false)
    {
        Roles.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Gets a list of roles from the database.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the roles.</param>
    /// <returns>A list of roles.</returns>
    public List<CustomRole> GetRoles(Expression<Func<CustomRole, bool>>? predicate = null)
    {
        if (predicate != null)
            return Roles.Where(predicate).ToList();
        return Roles.ToList();
    }

    /// <summary>
    /// Finds a role in the database based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the role.</param>
    /// <returns>The found role, or null if no role is found.</returns>
    public CustomRole? FindRole(Expression<Func<CustomRole, bool>> predicate)
    {
        return Roles.FirstOrDefault(predicate);
    }

    #endregion

    #region Role Claims

    /// <summary>
    /// Adds a new role claim to the database asynchronously.
    /// </summary>
    /// <param name="value">The role claim to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the role claim.</param>
    /// <returns>The added role claim.</returns>
    public async Task<IdentityRoleClaim<string>> AddRoleClaimAsync(IdentityRoleClaim<string> value,
        bool shouldSave = false)
    {
        var result = await RoleClaims.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Adds multiple role claims to the database asynchronously.
    /// </summary>
    /// <param name="value">The list of role claims to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the role claims.</param>
    public async Task AddRoleClaimsAsync(List<IdentityRoleClaim<string>> value, bool shouldSave = false)
    {
        await RoleClaims.AddRangeAsync(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing role claim in the database asynchronously.
    /// </summary>
    /// <param name="value">The role claim to update.</param>
    /// <param name="shouldSave">Whether to save changes after updating the role claim.</param>
    public async Task UpdateRoleClaimAsync(IdentityRoleClaim<string> value, bool shouldSave = false)
    {
        RoleClaims.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Removes an existing role claim from the database asynchronously.
    /// </summary>
    /// <param name="value">The role claim to remove.</param>
    /// <param name="shouldSave">Whether to save changes after removing the role claim.</param>
    public async Task RemoveRoleClaimAsync(IdentityRoleClaim<string> value, bool shouldSave = false)
    {
        RoleClaims.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }


    /// <summary>
    /// Gets a list of role claims from the database.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the role claims.</param>
    /// <returns>A list of role claims.</returns>
    public List<IdentityRoleClaim<string>> GetRoleClaims(
        Expression<Func<IdentityRoleClaim<string>, bool>>? predicate = null)
    {
        if (predicate != null)
            return RoleClaims.Where(predicate).ToList();
        return RoleClaims.ToList();
    }


    /// <summary>
    /// Finds a role claim in the database based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the role claim.</param>
    /// <returns>The found role claim, or null if no role claim is found.</returns>
    public IdentityRoleClaim<string>? FindRoleClaim(Expression<Func<IdentityRoleClaim<string>, bool>> predicate)
    {
        return RoleClaims.FirstOrDefault(predicate);
    }

    #endregion

    #region User Roles

    /// <summary>
    /// Adds a new user role to the database asynchronously.
    /// </summary>
    /// <param name="value">The user role to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the user role.</param>
    /// <returns>The added user role.</returns>
    public async Task<CustomUserRole> AddUserRoleAsync(CustomUserRole value, bool shouldSave = false)
    {
        var result = await UserRoles.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Updates an existing user role in the database asynchronously.
    /// </summary>
    /// <param name="value">The user role to update.</param>
    /// <param name="shouldSave">Whether to save changes after updating the user role.</param>
    public async Task UpdateUserRoleAsync(CustomUserRole value, bool shouldSave = false)
    {
        UserRoles.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Removes an existing user role from the database asynchronously.
    /// </summary>
    /// <param name="value">The user role to remove.</param>
    /// <param name="shouldSave">Whether to save changes after removing the user role.</param>
    public async Task RemoveUserRoleAsync(CustomUserRole value, bool shouldSave = false)
    {
        UserRoles.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Gets a list of user roles from the database.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the user roles.</param>
    /// <returns>A list of user roles.</returns>
    public List<CustomUserRole> GetUserRoles(Expression<Func<CustomUserRole, bool>>? predicate = null)
    {
        if (predicate != null)
            return UserRoles.Where(predicate).ToList();
        return UserRoles.ToList();
    }

    /// <summary>
    /// Finds a user role in the database based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the user role.</param>
    /// <returns>The found user role, or null if no user role is found.</returns>
    public CustomUserRole? FindUserRole(Expression<Func<CustomUserRole, bool>> predicate)
    {
        return UserRoles.FirstOrDefault(predicate);
    }

    #endregion

    #region User Tokens

    /// <summary>
    /// Adds a new user token to the database asynchronously.
    /// </summary>
    /// <param name="value">The user token to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the user token.</param>
    /// <returns>The added user token.</returns>
    public async Task<CustomUserToken> AddUserTokenAsync(CustomUserToken value, bool shouldSave = false)
    {
        var result = await UserTokens.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Updates an existing user token in the database asynchronously.
    /// </summary>
    /// <param name="value">The user token to update.</param>
    /// <param name="shouldSave">Whether to save changes after updating the user token.</param>
    public async Task UpdateUserTokenAsync(CustomUserToken value, bool shouldSave = false)
    {
        UserTokens.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Removes an existing user token from the database asynchronously.
    /// </summary>
    /// <param name="value">The user token to remove.</param>
    /// <param name="shouldSave">Whether to save changes after removing the user token.</param>
    public async Task RemoveUserTokenAsync(CustomUserToken value, bool shouldSave = false)
    {
        UserTokens.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Gets a list of user tokens from the database.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the user tokens.</param>
    /// <returns>A list of user tokens.</returns>
    public List<CustomUserToken> GetUserTokens(Expression<Func<CustomUserToken, bool>>? predicate = null)
    {
        if (predicate != null)
            return UserTokens.Where(predicate).ToList();
        return UserTokens.ToList();
    }

    /// <summary>
    /// Finds a user token in the database based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the user token.</param>
    /// <returns>The found user token, or null if no user token is found.</returns>
    public CustomUserToken? FindUserToken(Expression<Func<CustomUserToken, bool>> predicate)
    {
        return UserTokens.FirstOrDefault(predicate);
    }

    #endregion

    #region User Logins

    /// <summary>
    /// Adds a new user login to the database asynchronously.
    /// </summary>
    /// <param name="value">The user login to add.</param>
    /// <param name="shouldSave">Whether to save changes after adding the user login.</param>
    /// <returns>The added user login.</returns>
    public async Task<CustomUserLogin> AddUserLoginAsync(CustomUserLogin value, bool shouldSave = false)
    {
        var result = await UserLogins.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Updates an existing user login in the database asynchronously.
    /// </summary>
    /// <param name="value">The user login to update.</param>
    /// <param name="shouldSave">Whether to save changes after updating the user login.</param>
    public async Task UpdateUserLoginAsync(CustomUserLogin value, bool shouldSave = false)
    {
        UserLogins.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Removes an existing user login from the database asynchronously.
    /// </summary>
    /// <param name="value">The user login to remove.</param>
    /// <param name="shouldSave">Whether to save changes after removing the user login.</param>
    public async Task RemoveUserLoginAsync(CustomUserLogin value, bool shouldSave = false)
    {
        UserLogins.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }


    /// <summary>
    /// Gets a list of user logins from the database.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the user logins.</param>
    /// <returns>A list of user logins.</returns>
    public List<CustomUserLogin> GetUserLogins(Expression<Func<CustomUserLogin, bool>>? predicate = null)
    {
        if (predicate != null)
            return UserLogins.Where(predicate).ToList();
        return UserLogins.ToList();
    }

    /// <summary>
    /// Finds a user login in the database based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the user login.</param>
    /// <returns>The found user login, or null if no user login is found.</returns>
    public CustomUserLogin? FindUserLogin(Expression<Func<CustomUserLogin, bool>> predicate)
    {
        return UserLogins.FirstOrDefault(predicate);
    }

    
    public async Task ClearExpiredUserLoginsAsync(bool shouldSave = false)
    {
        var expiredLogins = await UserLogins.Where(l => l.ExpireDate <= DateTimeOffset.UtcNow).ToListAsync();
        foreach (var login in expiredLogins)
        {
            var token = UserTokens.FirstOrDefault(x => x.Id == login.ProviderKey);
            if (token != null)
                UserTokens.Remove(token);
        }
        
        UserLogins.RemoveRange(expiredLogins);
        if (shouldSave) await SaveChangesAsync();
    }
    #endregion

    #region User Billing Information

    /// <summary>
    /// Adds a new user billing information record to the database asynchronously.
    /// </summary>
    /// <param name="value">The user billing information to add.</param>
    /// <param name="shouldSave">Indicates whether to save changes to the database after adding the record.</param>
    /// <returns>The added user billing information.</returns>
    public async Task<UserBillingInformation> AddUserBillingInfoAsync(UserBillingInformation value,
        bool shouldSave = false)
    {
        var result = await UserBillingInformations.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Updates an existing user billing information record in the database asynchronously.
    /// </summary>
    /// <param name="value">The user billing information to update.</param>
    /// <param name="shouldSave">Indicates whether to save changes to the database after updating the record.</param>
    public async Task UpdateUserBillingInfoAsync(UserBillingInformation value, bool shouldSave = false)
    {
        UserBillingInformations.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Removes an existing user billing information record from the database asynchronously.
    /// </summary>
    /// <param name="value">The user billing information to remove.</param>
    /// <param name="shouldSave">Indicates whether to save changes to the database after removing the record.</param>
    public async Task RemoveUserBillingInfoAsync(UserBillingInformation value, bool shouldSave = false)
    {
        UserBillingInformations.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a list of user billing information records from the database.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the records.</param>
    /// <returns>A list of user billing information records.</returns>
    public async Task<List<UserBillingInformation>> GetUserBillingInfosAsync(
        Expression<Func<UserBillingInformation, bool>>? predicate = null)
    {
        if (predicate != null)
            return await UserBillingInformations.Where(predicate).ToListAsync();
        return await UserBillingInformations.ToListAsync();
    }

    /// <summary>
    /// Finds a specific user billing information record in the database based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the record.</param>
    /// <returns>The found user billing information record, or null if no record is found.</returns>
    public async Task<UserBillingInformation?> FindUserBillingInfoAsync(Expression<Func<UserBillingInformation, bool>> predicate)
    {
        return await UserBillingInformations.FirstOrDefaultAsync(predicate);
    }

    #endregion
    
    #region User Play Sessions

    public async Task<UserPlaySession> AddUserPlaySessionAsync(UserPlaySession value,
        bool shouldSave = false)
    {
        var result = await UserPlaySessions.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }
     
    public async Task UpdateUserPlaySessionAsync(UserPlaySession value, bool shouldSave = false)
    {
        UserPlaySessions.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task RemoveUserPlaySessionAsync(UserPlaySession value, bool shouldSave = false)
    {
        UserPlaySessions.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task<List<UserPlaySession>> GetUserPlaySessionsAsync(
        Expression<Func<UserPlaySession, bool>>? predicate = null)
    {
        if (predicate != null)
            return await UserPlaySessions.Where(predicate).ToListAsync();
        return await UserPlaySessions.ToListAsync();
    }
    
    public async Task<UserPlaySession?> FindUserPlaySessionAsync(Expression<Func<UserPlaySession, bool>> predicate)
    {
        return await UserPlaySessions.FirstOrDefaultAsync(predicate);
    }

    
    public async Task ClearExpiredUserPlaySessionsAsync(bool shouldSave = false)
    {
        var expiredSessions = await UserPlaySessions.Where(s => s.ExpiresAt <= DateTimeOffset.UtcNow).ToListAsync();
        UserPlaySessions.RemoveRange(expiredSessions);
        if (shouldSave) await SaveChangesAsync();
    }
    #endregion

    #region User Capes

     public async Task<UserCape> AddUserCapeAsync(UserCape value,
        bool shouldSave = false)
    {
        var result = await UserCapes.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }
     
    public async Task UpdateUserCapeAsync(UserCape value, bool shouldSave = false)
    {
        UserCapes.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task RemoveUserCapeAsync(UserCape value, bool shouldSave = false)
    {
        UserCapes.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task<List<UserCape>> GetUserCapesAsync(
        Expression<Func<UserCape, bool>>? predicate = null)
    {
        if (predicate != null)
            return await UserCapes.Where(predicate).ToListAsync();
        return await UserCapes.ToListAsync();
    }
    
    public async Task<UserCape?> FindUserCapeAsync(Expression<Func<UserCape, bool>> predicate)
    {
        return await UserCapes.FirstOrDefaultAsync(predicate);
    }

    #endregion
    
    /// <summary>
    /// Clears all user logins and tokens associated with the specified user ID.
    /// </summary>
    /// <param name="userId">The ID of the user whose logins and tokens are to be cleared.</param>
    /// <param name="shouldSave">Indicates whether to save changes to the database after clearing the logins and tokens.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task ClearUserLoginsAsync(string userId, bool shouldSave = false)
    {
        var logins = await UserLogins.Where(x => x.UserId == userId).ToListAsync();
        var tokens = await UserTokens.Where(x => x.UserId == userId).ToListAsync();

        foreach (var login in logins)
            await RemoveUserLoginAsync(login);

        foreach (var token in tokens)
            await RemoveUserTokenAsync(token);

        if (shouldSave)
            await SaveChangesAsync();
    }

    #endregion

    #region FileData
    public async Task<FileData> AddFileDataAsync(FileData value, bool shouldSave = false)
    {
        var result = await Files.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }
    
    public async Task UpdateFileDataAsync(FileData value, bool shouldSave = false)
    {
        Files.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task RemoveFileDataAsync(FileData value, bool shouldSave = false)
    {
        Files.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task<List<FileData>> GetFileDatasAsync(
        Expression<Func<FileData, bool>>? predicate = null)
    {
        if (predicate == null)
            return await Files.ToListAsync();
        return await Files.Where(predicate).ToListAsync();
    }
    
    public async Task<FileData?> FindFileDataAsync(Expression<Func<FileData, bool>>? predicate = null)
    {
        return await Files.FirstOrDefaultAsync(predicate!);
    }
    #endregion

    #region Capes

    public async Task<Cape> AddCapeAsync(Cape value,
        bool shouldSave = false)
    {
        var result = await Capes.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }
     
    public async Task UpdateCapeAsync(Cape value, bool shouldSave = false)
    {
        Capes.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task RemoveCapeAsync(Cape value, bool shouldSave = false)
    {
        Capes.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task<List<Cape>> GetCapesAsync(
        Expression<Func<Cape, bool>>? predicate = null)
    {
        if (predicate != null)
            return await Capes.Where(predicate).ToListAsync();
        return await Capes.ToListAsync();
    }
    
    public async Task<Cape?> FindCapeAsync(Expression<Func<Cape, bool>> predicate)
    {
        return await Capes.FirstOrDefaultAsync(predicate);
    }

    #endregion
    
    #region Server Joins

    public async Task<ServerJoin> AddServerJoinAsync(ServerJoin value,
        bool shouldSave = false)
    {
        var result = await ServerJoins.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }
     
    public async Task UpdateServerJoinAsync(ServerJoin value, bool shouldSave = false)
    {
        ServerJoins.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task RemoverServerJoinAsync(ServerJoin value, bool shouldSave = false)
    {
        ServerJoins.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    public async Task<List<ServerJoin>> GetServerJoinsAsync(
        Expression<Func<ServerJoin, bool>>? predicate = null)
    {
        if (predicate != null)
            return await ServerJoins.Where(predicate).ToListAsync();
        return await ServerJoins.ToListAsync();
    }
    
    public async Task<ServerJoin?> FindServerJoinAsync(Expression<Func<ServerJoin, bool>> predicate)
    {
        return await ServerJoins.FirstOrDefaultAsync(predicate);
    }

    public async Task ClearExpiredServerJoinsAsync(bool shouldSave = false)
    {
        var expiredJoins = await ServerJoins.Where(sj => sj.ExpiresAt <= DateTimeOffset.UtcNow).ToListAsync();
        ServerJoins.RemoveRange(expiredJoins);
        if (shouldSave) await SaveChangesAsync();
    }
    #endregion
    
    #region News
    /// <summary>
    /// Adds a new news entry to the database asynchronously.
    /// </summary>
    /// <param name="value">The news entry to add.</param>
    /// <param name="shouldSave">Indicates whether to save changes to the database after adding the entry.</param>
    /// <returns>The added news entry.</returns>
    public async Task<News> AddNewsAsync(News value, bool shouldSave = false)
    {
        var result = await News.AddAsync(value);
        if (shouldSave) await SaveChangesAsync();
        return result.Entity;
    }

    /// <summary>
    /// Updates an existing news entry in the database asynchronously.
    /// </summary>
    /// <param name="value">The news entry to update.</param>
    /// <param name="shouldSave">Indicates whether to save changes to the database after updating the entry.</param>
    public async Task UpdateNewsAsync(News value, bool shouldSave = false)
    {
        News.Update(value);
        if (shouldSave) await SaveChangesAsync();
    }
    
    /// <summary>
    /// Removes an existing news entry from the database asynchronously.
    /// </summary>
    /// <param name="value">The news entry to remove.</param>
    /// <param name="shouldSave">Indicates whether to save changes to the database after removing the entry.</param>
    public async Task RemoveNewsAsync(News value, bool shouldSave = false)
    {
        News.Remove(value);
        if (shouldSave) await SaveChangesAsync();
    }

    /// <summary>
    /// Retrieves a list of news entries from the database asynchronously.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter the news entries.</param>
    /// <returns>A list of news entries.</returns>
    public async Task<List<News>> GetNewsAsync(
        Expression<Func<News, bool>>? predicate = null)
    {
        if (predicate == null)
            return await News.ToListAsync();
        return await News.Where(predicate).ToListAsync();
    }
    
    /// <summary>
    /// Retrieves the latest news entries from the database asynchronously.
    /// </summary>
    /// <param name="count">The number of latest news entries to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of the latest news entries.</returns>
    public async Task<List<News>> GetLatestNewsAsync(int count)
    {
        return await News.OrderByDescending(n => n.CreatedAt).Take(count).ToListAsync();
    }

    /// <summary>
    /// Finds a specific news entry in the database asynchronously based on a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to filter the news entry.</param>
    /// <returns>The found news entry, or null if no entry is found.</returns>
    public async Task<News?> FindNewsAsync(Expression<Func<News, bool>>? predicate = null)
    {
        return await News.FirstOrDefaultAsync(predicate!);
    }
    #endregion
}