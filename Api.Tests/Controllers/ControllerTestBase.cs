using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Tests.Helpers;
using Tavstal.MesterMC.Api.Utils.Helpers;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers;

public abstract class ControllerTestBase
{
    protected readonly ITestOutputHelper _testOutputHelper;
    protected readonly CustomDbContext _dbContext;
    protected readonly UserManager<CustomUser> _userManager;
    protected readonly DefaultHttpContext _controllerHttpContext;
    protected readonly MemoryCacheService _memoryCacheService;
    protected readonly Settings _settings;
    protected readonly CustomUser _userMock;
    protected readonly CustomUser _userMock2;
    protected const string _passwordMock = "This%Valid_And#Pass%mock-2026";

    protected ControllerTestBase(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        _dbContext = TestHelper.CreateInMemoryDbContext();
        _userManager = TestHelper.CreateCustomUserManager(_dbContext);
        _memoryCacheService = TestHelper.MemoryCacheService;
        _settings = TestHelper.CreateTestSettings();

        var uploadTempDir = Path.Combine(Path.GetTempPath(), "mmc-tests-uploads");
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["UploadDirectory"] = uploadTempDir
        }).Build();
        _ = new Startup(config);
        
        _controllerHttpContext = new DefaultHttpContext
        {
            Connection = { RemoteIpAddress = IPAddress.Parse(TestHelper.IpAddress) }
        };
        _controllerHttpContext.Request.Headers.UserAgent = TestHelper.UserAgent;
        _controllerHttpContext.Request.Host = new HostString("localhost", 5000);
        
        _userMock = new CustomUser
        {
            Email = "testuser@gmail.com",
            EmailConfirmed = true,
            NormalizedEmail = "testuser@gmail.com".Normalize(),
            UserName = "testuser",
            NormalizedUserName = "testuser".Normalize(),
            PasswordHash = StringChiper.GetEncryptedSha256Hash(_passwordMock, _settings.EncryptionKey),
            CreateDate = DateTimeOffset.UtcNow,
            LastLogin = DateTimeOffset.UtcNow,
            LastUpdate = DateTimeOffset.UtcNow,
            SkinModel = ESkinType.WIDE,
            LockoutEnabled = false,
        };
        _userMock2 = new CustomUser
        {
            Email = "testuser2@gmail.com",
            EmailConfirmed = true,
            NormalizedEmail = "testuser2@gmail.com".Normalize(),
            UserName = "testuser2",
            NormalizedUserName = "testuser2".Normalize(),
            PasswordHash = StringChiper.GetEncryptedSha256Hash(_passwordMock, _settings.EncryptionKey),
            CreateDate = DateTimeOffset.UtcNow,
            LastLogin = DateTimeOffset.UtcNow,
            LastUpdate = DateTimeOffset.UtcNow,
            SkinModel = ESkinType.WIDE,
            LockoutEnabled = false,
        };
    }
    
    protected async Task CreateUserAsync(Controller controller, CustomUser? user = null, bool givePermissions = true)
    {
        user ??= _userMock;
        await _userManager.CreateAsync(user, _passwordMock);
        if (givePermissions)
        {
            var adminRole = await _dbContext.AddRoleAsync(new CustomRole(3, "admin", "ADMIN"), true);
            await _dbContext.AddUserRoleAsync(new CustomUserRole
            {
                UserId = user.Id,
                RoleId = adminRole.Id
            }, true);

            var adminClaims = CustomRoleClaims.Claims["Admin"];
            foreach (var claim in adminClaims)
                await _dbContext.AddRoleClaimAsync(new IdentityRoleClaim<string>()
                {
                    RoleId = adminRole.Id,
                    ClaimType = claim.Key,
                    ClaimValue = claim.DefaultValue
                }, true);
        }
        
        var role = await _dbContext.AddRoleAsync(new CustomRole(1, "default", "DEFAULT"), true);
        await _dbContext.AddUserRoleAsync(new CustomUserRole
        {
            UserId = user.Id,
            RoleId = role.Id
        }, true);
        
        var defaultClaims = CustomRoleClaims.Claims["Default"];
        foreach (var claim in defaultClaims)
            await _dbContext.AddRoleClaimAsync(new IdentityRoleClaim<string>()
            {
                RoleId = role.Id,
                ClaimType = claim.Key,
                ClaimValue = claim.DefaultValue
            }, true);
        await _dbContext.SaveChangesAsync();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName)
        };
        _controllerHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        controller.ControllerContext.HttpContext = _controllerHttpContext;
    }
}