using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Models.Database.User;

/// <summary>
/// Represents a custom implementation of the UserStore class, which provides 
/// the APIs for managing user data in a persistence store.
/// </summary>
/// <remarks>
/// This class extends the generic UserStore class with custom user and role types, 
/// as well as additional entity types for claims, roles, logins, tokens, and role claims.
/// </remarks>
public class CustomUserStore : UserStore<CustomUser, CustomRole, CustomDbContext, string, CustomUserClaim, CustomUserRole, IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomUserStore"/> class.
    /// </summary>
    /// <param name="context">The database context used to access the persistence store.</param>
    /// <param name="describer">An optional <see cref="IdentityErrorDescriber"/> to provide error descriptions.</param>
    public CustomUserStore(CustomDbContext context, IdentityErrorDescriber? describer = null)
        : base(context, describer)
    {
    }
}
