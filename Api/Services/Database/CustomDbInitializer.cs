using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Models.Database.User.Claims;

namespace Tavstal.MesterMC.Api.Services.Database;

/// <summary>
/// Provides functionality to initialize the custom database context.
/// </summary>
public static class CustomDbInitializer
{
    /// <summary>
    /// Initializes the database by ensuring it is created and populating default data.
    /// </summary>
    /// <param name="context">The custom database context to initialize.</param>
    /// <param name="userStore">The custom user store used for managing user data.</param>
    public static void Initialize(CustomDbContext context, CustomUserStore userStore)
    {
        // Ensures the database is created.
        context.Database.EnsureCreated();
        
        Task.Run(async () =>
        {
            // Checks if roles are empty and adds default roles.
            var roles = await userStore.Roles.QueryAsync(null);
            if (!roles.Any())
            {
                 await userStore.Roles.AddRangeAsync([
                    new(1, "Default", "DEFAULT"),
                    new (90, "Moderator", "MODERATOR"),
                    new(100, "Admin", "ADMIN"),
                ], true);

                Dictionary<string, CustomRole> roleCache = new();
                 
                // Adds claims to roles based on predefined role claims.
                foreach (var roleClaims in CustomRoleClaims.Claims)
                {
                    if (!roleCache.TryGetValue(roleClaims.Key, out CustomRole? role))
                    {
                        role = await userStore.Roles.FindAsync(x => x.NormalizedName == roleClaims.Key);
                        if (role == null)
                            continue;
                        roleCache[roleClaims.Key] = role;
                    }

                    await userStore.RoleClaims.AddRangeAsync(RoleClaim.ToList(roleClaims.Value.ToList(), role.Id));
                }
                await context.SaveChangesAsync();
            }
        }).GetAwaiter().GetResult();
    }
}