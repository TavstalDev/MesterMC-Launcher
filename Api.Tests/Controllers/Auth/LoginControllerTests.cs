using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
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

    public LoginControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var loggerMock = new Mock<ILogger<LoginController>>();
        _dbContext = TestHelper.CreateInMemoryDbContext();
        var userManager = TestHelper.CreateCustomUserManager(_dbContext);
        _emailService = TestHelper.CreateEmailService();
        _settings = TestHelper.CreateTestSettings();
        _controller = new LoginController(loggerMock.Object, _dbContext, userManager, _emailService, _settings);
    }

    public class LoginTests : LoginControllerTests
    {
        public LoginTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact]
        public async Task ReturnsOk()
        {
            // Add mock user
            await _dbContext.AddUserAsync(new CustomUser
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
            }, true);
            
            var httpContext = new DefaultHttpContext
            {
                Connection =
                {
                    RemoteIpAddress = IPAddress.Parse("127.0.0.1")
                }
            };

            // Set User-Agent header
            httpContext.Request.Headers.UserAgent = "UnitTestAgent/1.0";
            httpContext.Request.Host = new HostString("localhost", 5000);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody()
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