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
    public static void Initialize(CustomDbContext context)
    {
        // Ensures the database is created.
        context.Database.EnsureCreated();
        
        Task.Run(async () =>
        {
            // Checks if roles are empty and adds default roles.
            if (context.Roles.ToList().Count == 0)
            {
                context.Roles.AddRange(new List<CustomRole>
                {
                    new(1, "Default", "DEFAULT"),
                    new(1, "Restricted", "RESTRICTED"),
                    new(1, "UnderTermination", "UNDER_TERMINATION"),
                    new (2, "Moderator", "MODERATOR"),
                    new(3, "Admin", "ADMIN"),
                });
                await context.SaveChangesAsync();

                // Adds claims to roles based on predefined role claims.
                foreach (var roleClaims in CustomRoleClaims.Claims)
                {
                    var role = context.FindRole(x => x.Name == roleClaims.Key);
                    if (role == null)
                        continue;

                    context.RoleClaims.AddRange(RoleClaim.ToList(roleClaims.Value.ToList(), role.Id));
                }
                await context.SaveChangesAsync();
            }
        }).GetAwaiter().GetResult();
    }
}