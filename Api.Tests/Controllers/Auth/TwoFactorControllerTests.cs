using System.Net;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OtpNet;
using Tavstal.MesterMC.Api.Controllers.Auth;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Tests.Helpers;
using Tavstal.MesterMC.Api.Utils.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Auth;

public class TwoFactorControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomDbContext _dbContext;
    private readonly TwoFactorController _controller;
    private readonly DefaultHttpContext _controllerHttpContext;
    private readonly CustomUser _userMock;
    private const string _passwordMock = "This%Valid_And#Pass%mock-2026";

    public TwoFactorControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var loggerMock = new Mock<ILogger<TwoFactorController>>();
        _dbContext = TestHelper.CreateInMemoryDbContext();
        var userManager = TestHelper.CreateCustomUserManager(_dbContext);
        var emailService = TestHelper.FakeEmailService;
        var settings = TestHelper.CreateTestSettings();
        _controller = new TwoFactorController(loggerMock.Object, userManager, _dbContext, emailService, settings);

        _controllerHttpContext = new DefaultHttpContext
        {
            Connection =
            {
                RemoteIpAddress = IPAddress.Parse(TestHelper.IpAddress)
            }
        };
        _controllerHttpContext.Request.Headers.UserAgent = TestHelper.UserAgent;
        _controllerHttpContext.Request.Host = new HostString("localhost", 5000);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };

        _userMock = new CustomUser
        {
            Email = "testuser@gmail.com",
            EmailConfirmed = true,
            NormalizedEmail = "testuser@gmail.com".Normalize(),
            UserName = "testuser",
            NormalizedUserName = "testuser".Normalize(),
            PasswordHash = StringChiper.GetEncryptedSha256Hash(_passwordMock, settings.EncryptionKey),
            CreateDate = DateTimeOffset.UtcNow,
            LastLogin = DateTimeOffset.UtcNow,
            LastUpdate = DateTimeOffset.UtcNow,
            SkinModel = ESkinType.WIDE
        };
    }

    public class EnableTwoFactorTests : TwoFactorControllerTests
    {
        public EnableTwoFactorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact(DisplayName = "Success: Enable 2FA")]
        public async Task ReturnsOk()
        {
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);

            byte[] secretBytes = Encoding.UTF8.GetBytes(_userMock.TwoFactorSecret);
            var totp = new Totp(secretBytes);
            string code = totp.ComputeTotp();
            
            IActionResult result = await _controller.EnableTwoFactorAuthAsync(code);
            
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }

        [Fact(DisplayName = "Failure: Invalid code")]
        public async Task ReturnsUnauthorized_ForInvalidCode()
        {
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);

            IActionResult result = await _controller.EnableTwoFactorAuthAsync("000000");
            
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }

        [Fact(DisplayName = "Failure: Unauthenticated user")]
        public async Task ReturnsUnauthorized_WhenUnauthenticated()
        {
            IActionResult result = await _controller.EnableTwoFactorAuthAsync("000000");
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
        
        [Fact(DisplayName = "Failure: TFA already enabled")]
        public async Task ReturnsForbidden()
        {
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);

            IActionResult result = await _controller.EnableTwoFactorAuthAsync("000000");
            
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
    }

    public class DisableTwoFactorTests : TwoFactorControllerTests
    {
        public DisableTwoFactorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Disable 2FA with valid code")]
        public async Task ReturnsOk()
        {
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);

            byte[] secretBytes = Encoding.UTF8.GetBytes(_userMock.TwoFactorSecret);
            var totp = new Totp(secretBytes);
            string code = totp.ComputeTotp();
            
            IActionResult result = await _controller.DisableTwoFactorAuthAsync(code);
            
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }

        [Fact(DisplayName = "Failure: Invalid code")]
        public async Task ReturnsUnauthorized_ForInvalidCode()
        {
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);
            
            IActionResult result = await _controller.DisableTwoFactorAuthAsync("000000");

            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
        
        [Fact(DisplayName = "Failure: Unauthenticated user")]
        public async Task ReturnsUnauthorized_WhenUnauthenticated()
        {
            IActionResult result = await _controller.DisableTwoFactorAuthAsync("000000");
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
        
        [Fact(DisplayName = "Failure: TFA not enabled")]
        public async Task ReturnsForbidden()
        {
            _userMock.TwoFactorEnabled = false;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);
            
            IActionResult result = await _controller.DisableTwoFactorAuthAsync("000000");

            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
    }
    
    public class GenerateCodeTests : TwoFactorControllerTests
    {
        public GenerateCodeTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Generates new 2FA secret")]
        public async Task ReturnsOk()
        {
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);
            
            IActionResult result = await _controller.GenerateCodeAsync();
            
            result.Should().BeOfType<ContentResult>();
            var content = (result as ContentResult)!.Content;
            _testOutputHelper.WriteLine("Result: " + content);
        }

        [Fact(DisplayName = "Failure: Unauthorized user")]
        public async Task ReturnsUnauthorized()
        {
            IActionResult result = await _controller.GenerateCodeAsync();
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
        
        [Fact(DisplayName = "Failure: TFA already enabled")]
        public async Task ReturnsForbidden()
        {
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);
            
            IActionResult result = await _controller.GenerateCodeAsync();
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
    }

    public class RegenerateRecoveryCodesTests : TwoFactorControllerTests
    {
        public RegenerateRecoveryCodesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Regenerate recovery codes")]
        public async Task ReturnsOk()
        {
            await _dbContext.AddUserAsync(_userMock, true);
            await AuthenticateAsUserAsync(_userMock);
            
            IActionResult result = await _controller.RegenerateRecoveryCodesAsync();
            
            result.Should().BeOfType<ContentResult>();
            var obj = result as ContentResult;
            obj.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + obj.Content);
        }

        [Fact(DisplayName = "Failure: Unauthenticated user")]
        public async Task ReturnsUnauthorized_WhenUnauthenticated()
        {
            IActionResult result = await _controller.RegenerateRecoveryCodesAsync();
            
            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
    }
    
    private async Task AuthenticateAsUserAsync(CustomUser user, bool isAdmin = false)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName)
        };
        _controllerHttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _controller.ControllerContext.HttpContext = _controllerHttpContext;
        
        if (!isAdmin)
            return;
        
        var adminRole = _dbContext.FindRole(x => x.NormalizedName == "ADMIN");
        adminRole.Should().NotBeNull("Admin role should exist in the database");
        await _dbContext.AddUserRoleAsync(new CustomUserRole
        {
            UserId = user.Id,
            RoleId = adminRole.Id
        }, true);
    }
}