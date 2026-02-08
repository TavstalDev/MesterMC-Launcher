using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Models.Database.User;

public class CustomUserStore : UserStore<CustomUser, CustomRole, CustomDbContext, ulong, CustomUserClaim, CustomUserRole, IdentityUserLogin<ulong>, IdentityUserToken<ulong>, IdentityRoleClaim<ulong>>
{
    public CustomUserStore(CustomDbContext context, IdentityErrorDescriber? describer = null)
        : base(context, describer)
    {
    }
}