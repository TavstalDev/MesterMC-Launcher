using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OtpNet;
using Tavstal.MesterMC.Api.Controllers.Auth;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Bodies.Auth;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Tests.Helpers;
using Tavstal.MesterMC.Api.Tests.Services;
using Tavstal.MesterMC.Api.Utils.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Auth;

public class LoginControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomDbContext _dbContext;
    private readonly FakeEmailService _emailService;
    private readonly Settings _settings;
    private readonly LoginController _controller;
    private readonly DefaultHttpContext _controllerHttpContext;

    private readonly CustomUser _userMock;

    public LoginControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var loggerMock = new Mock<ILogger<LoginController>>();
        _dbContext = TestHelper.CreateInMemoryDbContext();
        var userManager = TestHelper.CreateCustomUserManager(_dbContext);
        _emailService = TestHelper.CreateEmailService();
        _settings = TestHelper.CreateTestSettings();
        _controller = new LoginController(loggerMock.Object, _dbContext, userManager, _emailService, _settings);
        _controllerHttpContext = new DefaultHttpContext
        {
            Connection =
            {
                RemoteIpAddress = IPAddress.Parse("127.0.0.1")
            }
        };
        // Set User-Agent header
        _controllerHttpContext.Request.Headers.UserAgent = "UnitTestAgent/1.0";
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
            PasswordHash = StringChiper.GetEncryptedSha256Hash("This%Valid_And#Pass%mock-2026", _settings.EncryptionKey),
            CreateDate = DateTimeOffset.UtcNow,
            LastLogin = DateTimeOffset.UtcNow,
            LastUpdate = DateTimeOffset.UtcNow,
            SkinModel = ESkinType.WIDE
        };
    }

    public class LoginTests : LoginControllerTests
    {
        public LoginTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Login with valid credentials")]
        public async Task ReturnsOk()
        {
            // Add mock user
            await _dbContext.AddUserAsync(_userMock, true);
            
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody
            {
                Email = "testuser@gmail.com",
                Password = "This%Valid_And#Pass%mock-2026"
            });

            result.Should().BeOfType<ContentResult>();

            ContentResult? contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Login Result: " + contentResult.Content);
        }
        
        [Fact(DisplayName = "Success: Login with user TFA enabled")]
        public async Task ReturnsRedirect()
        {
            // Add mock user
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            
            await _dbContext.AddUserAsync(_userMock, true);
            
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody
            {
                Email = "testuser@gmail.com",
                Password = "This%Valid_And#Pass%mock-2026"
            });

            result.Should().BeOfType<ContentResult>();

            ContentResult? contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Login Result: " + contentResult.Content);
        }

        [Fact(DisplayName = "Failure: Login with non-existent user")]
        public async Task ReturnsNotFound()
        {
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody
            {
                Email = "testuser@gmail.com",
                Password = "This%Valid_And#Pass%mock-2026"
            });

            result.Should().BeOfType<ObjectResult>();

            ObjectResult? contentResult = result as ObjectResult;
            contentResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Login Result: " + contentResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Login with incorrect password")]
        public async Task ReturnsUnauthorized()
        {
            // Add mock user
            await _dbContext.AddUserAsync(_userMock, true);
            
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody
            {
                Email = "testuser@gmail.com",
                Password = "This%Valid_And#Pass%mock-2027"
            });

            result.Should().BeOfType<ObjectResult>();

            ObjectResult? contentResult = result as ObjectResult;
            contentResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Login Result: " + contentResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Login with locked out user")]
        public async Task ReturnsLocked()
        {
            // Add mock user
            _userMock.LockoutEnabled = true;
            _userMock.LockoutEnd = DateTime.UtcNow.AddDays(30);
            _userMock.LockoutReason = "Too many failed login attempts";
            
            await _dbContext.AddUserAsync(_userMock, true);
            
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody
            {
                Email = "testuser@gmail.com",
                Password = "This%Valid_And#Pass%mock-2026"
            });

            result.Should().BeOfType<ContentResult>();

            ContentResult? contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Login Result: " + contentResult.Content);
        }
    }

    public class LoginTwoFactorTests : LoginControllerTests
    {
        public LoginTwoFactorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Login with valid credentials")]
        public async Task ReturnsOk()
        {
            // Add mock user
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody
            {
                Email = "testuser@gmail.com",
                Password = "This%Valid_And#Pass%mock-2026"
            });

            result.Should().BeOfType<ContentResult>();
            var setCookieHeader = _controllerHttpContext.Response.Headers.SetCookie.ToString();
            setCookieHeader.Should().Contain("mmc-twofactor-session=");
            
            var setCookie = _controllerHttpContext.Response.Headers.SetCookie.ToString();
            if (!string.IsNullOrEmpty(setCookie))
            {
                // take only the first "name=value" segment before any attributes
                var cookiePair = setCookie.Split(';', 2)[0].Trim();
                _controllerHttpContext.Request.Headers["Cookie"] = cookiePair;
            }

            ContentResult? contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Login Redirect Result: " + contentResult.Content);

            byte[] secretBytes = Encoding.UTF8.GetBytes(_userMock.TwoFactorSecret);
            var totpGenerator = new Totp(secretBytes);
            string expectedCode = totpGenerator.ComputeTotp();
            result = await _controller.LoginTwoFactorAsync(new LoginTFASessionRequestBody
            {
                TwoFactorCode = expectedCode
            });
            
            result.Should().BeOfType<ContentResult>();

            contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Final Result: " + contentResult.Content);
        }
    }

    public class LoginLauncherTests : LoginControllerTests
    {
        public LoginLauncherTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
    }
    
    public class LoginTwoFactorLauncherTests : LoginControllerTests
    {
        public LoginTwoFactorLauncherTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
    }
    
    public class LogoutTests : LoginControllerTests
    {
        public LogoutTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
    }
}