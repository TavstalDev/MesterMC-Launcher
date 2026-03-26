using System.Net;
using System.Text;
using System.Globalization;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using OtpNet;
using Tavstal.MesterMC.Api.Controllers.Auth;
using Tavstal.MesterMC.Api.Models.Bodies.Auth;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Tests.Helpers;
using Tavstal.MesterMC.Api.Utils.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Auth;

public class LoginControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomDbContext _dbContext;
    private readonly LoginController _controller;
    private readonly DefaultHttpContext _controllerHttpContext;
    private readonly CustomUser _userMock;
    private const string _passwordMock = "This%Valid_And#Pass%mock-2026";

    public LoginControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var loggerMock = new Mock<ILogger<LoginController>>();
        _dbContext = TestHelper.CreateInMemoryDbContext();
        var userManager = TestHelper.CreateCustomUserManager(_dbContext);
        var emailService = TestHelper.CreateEmailService();
        var settings = TestHelper.CreateTestSettings();
        _controller = new LoginController(loggerMock.Object, _dbContext, userManager, emailService, settings);
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
            PasswordHash = StringChiper.GetEncryptedSha256Hash(_passwordMock, settings.EncryptionKey),
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
            string? result = await AddMockUserAndLoginAsync();
            result.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + result);
        }
        
        [Fact(DisplayName = "Redirect: TFA enabled")]
        public async Task ReturnsRedirect()
        {
            string? result = await AddMockUserAndLoginAsync(true);
            result.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + result);
        }

        [Fact(DisplayName = "Failure: Non-existent user")]
        public async Task ReturnsNotFound()
        {
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody
            {
                Email = _userMock.Email,
                Password = _passwordMock
            });

            result.Should().BeOfType<ObjectResult>();

            ObjectResult? contentResult = result as ObjectResult;
            contentResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + contentResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Incorrect password")]
        public async Task ReturnsUnauthorized()
        {
            await _dbContext.AddUserAsync(_userMock, true);
            IActionResult result = await _controller.LoginAsync(new LoginRequestBody
            {
                Email = _userMock.Email,
                Password = "This%Valid_And#Pass%mock-2027"
            });

            result.Should().BeOfType<ObjectResult>();
            ObjectResult? contentResult = result as ObjectResult;
            contentResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + contentResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Locked out user")]
        public async Task ReturnsLocked()
        {
            _userMock.LockoutEnabled = true;
            _userMock.LockoutEnd = DateTime.UtcNow.AddDays(30);
            _userMock.LockoutReason = "Too many failed login attempts";
            string? result = await AddMockUserAndLoginAsync();
            _testOutputHelper.WriteLine("Result: " + result);
        }
    }

    public class LoginTwoFactorTests : LoginControllerTests
    {
        public LoginTwoFactorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Login with valid credentials")]
        public async Task ReturnsOk()
        {
            await AddMockUserAndLoginAsync(true);
            var setCookieHeader = _controllerHttpContext.Response.Headers.SetCookie.ToString();
            setCookieHeader.Should().Contain("mmc-twofactor-session=");
            _userMock.TwoFactorSecret.Should().NotBeNullOrEmpty();

            byte[] secretBytes = Encoding.UTF8.GetBytes(_userMock.TwoFactorSecret);
            var totpGenerator = new Totp(secretBytes);
            string expectedCode = totpGenerator.ComputeTotp();
            IActionResult result = await _controller.LoginTwoFactorAsync(new LoginTFASessionRequestBody
            {
                TwoFactorCode = expectedCode
            });
            
            result.Should().BeOfType<ContentResult>();

            ContentResult? contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + contentResult.Content);
        }

        [Fact(DisplayName = "Failure: Missing TFA session cookie")]
        public async Task ReturnsUnauthorized_ForMissingCookie()
        {
            await AddMockUserAndLoginAsync(true);
            _controllerHttpContext.Request.Headers.Cookie = []; // Clear cookies
            IActionResult result = await _controller.LoginTwoFactorAsync(new LoginTFASessionRequestBody
            {
                TwoFactorCode = "000000"
            });
            
            result.Should().BeOfType<ObjectResult>();
            
            ObjectResult? objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Invalid TFA code")]
        public async Task ReturnsUnauthorized_ForInvalidCode()
        {
            await AddMockUserAndLoginAsync(true);
            
            IActionResult result = await _controller.LoginTwoFactorAsync(new LoginTFASessionRequestBody
            {
                TwoFactorCode = "000000"
            });
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Expired TFA session")]
        public async Task ReturnsForbidden_ForExpiredSession()
        {
            await AddMockUserAndLoginAsync(true);
            var setCookie = _controllerHttpContext.Response.Headers.SetCookie.ToString();
            setCookie.Should().Contain("mmc-twofactor-session=");

            if (!string.IsNullOrEmpty(setCookie))
            {
                var cookiePair = setCookie.Split(';', 2)[0].Trim();
                _controllerHttpContext.Request.Headers.Cookie = cookiePair;
                var cookieValue = cookiePair.Split('=', 2)[1];
                
                var sessionClaim = _dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.TwoFactorSessionToken && x.ClaimValue == cookieValue);
                sessionClaim.Should().NotBeNull();

                var expiryClaim = _dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.TwoFactorSessionExpiration && x.UserId == sessionClaim.UserId);
                expiryClaim.Should().NotBeNull();

                await _dbContext.RemoveUserClaimAsync(expiryClaim);
                await _dbContext.SetUserClaimAsync(new CustomUserClaim
                {
                    UserId = sessionClaim.UserId,
                    ClaimType = CustomClaimTypes.TwoFactorSessionExpiration,
                    ClaimValue = DateTimeOffset.UtcNow.AddMinutes(-10).ToString(CultureInfo.InvariantCulture)
                });
                await _dbContext.SaveChangesAsync();
            }
            
            IActionResult result = await _controller.LoginTwoFactorAsync(new LoginTFASessionRequestBody
            {
                TwoFactorCode = "000000"
            });

            result.Should().BeOfType<ObjectResult>();
            var obj = result as ObjectResult;
            obj!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + obj.Value);
        }
    }

    public class LoginLauncherTests : LoginControllerTests
    {
        public LoginLauncherTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        
        [Fact(DisplayName = "Success: Launcher login with valid credentials")]
        public async Task ReturnsOk()
        { 
            string? result = await AddMockUserAndLoginLauncherAsync();
            result.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + result);
        }
        
        [Fact(DisplayName = "Redirect: Launcher login with TFA")]
        public async Task ReturnsRedirect_WhenTwoFactorEnabled()
        {
            string? result = await AddMockUserAndLoginLauncherAsync(true);
            result.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + result);
        }

        [Fact(DisplayName = "Failure: Incorrect password")]
        public async Task ReturnsUnauthorized()
        {
            await _dbContext.AddUserAsync(_userMock, true);
            IActionResult result = await _controller.LoginLauncherAsync(new LauncherLoginRequestBody
            {
                Username = _userMock.UserName,
                Password = "wrong-password"
            });
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Invalid code returns unauthorized")]
        public async Task ReturnsUnauthorized_ForInvalidTwoFactorCode()
        {
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _dbContext.AddUserAsync(_userMock, true);
            
            IActionResult result = await _controller.LoginLauncherAsync(new LauncherLoginRequestBody
            {
                Username = _userMock.UserName,
                Password = _passwordMock,
                TwoFactorCode = "000000"
            });
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }

    public class LoginTwoFactorLauncherTests : LoginControllerTests
    {
        public LoginTwoFactorLauncherTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact(DisplayName = "Success: Launcher 2FA flow (redirect then confirm)")]
        public async Task ReturnsOk()
        {
            string? result = await AddMockUserAndLoginLauncherAsync(true);
            result.Should().NotBeNullOrEmpty();
            JObject json = JObject.Parse(result);
            string sessionToken = json["token"]?.ToString()!;
            sessionToken.Should().NotBeNullOrEmpty();
            _userMock.TwoFactorSecret.Should().NotBeNullOrEmpty();
            
            byte[] secretBytes = Encoding.UTF8.GetBytes(_userMock.TwoFactorSecret);
            var totpGenerator = new Totp(secretBytes);
            string expectedCode = totpGenerator.ComputeTotp();

            IActionResult secondResult = await _controller.LoginLauncherTwoFactorAsync(
                new LauncherLoginTFASessionRequestBody
                {
                    SessionToken = sessionToken,
                    TwoFactorCode = expectedCode
                });
            
            secondResult.Should().BeOfType<ContentResult>();
            var secondContent = (secondResult as ContentResult)!.Content;
            secondContent.Should().Contain("Login successful");
            _testOutputHelper.WriteLine("Result: " + secondContent);
        }

        [Fact(DisplayName = "Failure: Missing/invalid session token")]
        public async Task ReturnsUnauthorized_ForMissingToken()
        {
            await AddMockUserAndLoginLauncherAsync(true, false);
            IActionResult result = await _controller.LoginLauncherTwoFactorAsync(new LauncherLoginTFASessionRequestBody
            {
                SessionToken = "invalid-token",
                TwoFactorCode = "000000"
            });
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Invalid two-factor code")]
        public async Task ReturnsUnauthorized_ForInvalidCode()
        {
            string? result = await AddMockUserAndLoginLauncherAsync(true);
            result.Should().NotBeNullOrEmpty();
            JObject json = JObject.Parse(result);
            string sessionToken = json["token"]?.ToString()!;
            
            IActionResult secondResult = await _controller.LoginLauncherTwoFactorAsync(
                new LauncherLoginTFASessionRequestBody
                {
                    SessionToken = sessionToken,
                    TwoFactorCode = "000000"
                });
            
            secondResult.Should().BeOfType<ObjectResult>();
            var objectResult = secondResult as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Expired session token")]
        public async Task ReturnsForbidden_ForExpiredLauncherSession()
        {
            string? result = await AddMockUserAndLoginLauncherAsync(true);
            result.Should().NotBeNullOrEmpty();
            JObject json = JObject.Parse(result);
            string sessionToken = json["token"]?.ToString()!;
            
            var sessionClaim = _dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.TwoFactorLauncherSessionToken && x.ClaimValue == sessionToken);
            sessionClaim.Should().NotBeNull();

            var expiryClaim = _dbContext.FindUserClaim(x => x.ClaimType == CustomClaimTypes.TwoFactorLauncherSessionExpiration && x.UserId == sessionClaim.UserId);
            expiryClaim.Should().NotBeNull();

            await _dbContext.RemoveUserClaimAsync(expiryClaim);
            await _dbContext.SetUserClaimAsync(new CustomUserClaim
            {
                UserId = sessionClaim.UserId,
                ClaimType = CustomClaimTypes.TwoFactorLauncherSessionExpiration,
                ClaimValue = DateTimeOffset.UtcNow.AddMinutes(-10).ToString(CultureInfo.InvariantCulture)
            });
            await _dbContext.SaveChangesAsync();
            
            IActionResult secondResult = await _controller.LoginLauncherTwoFactorAsync(
                new LauncherLoginTFASessionRequestBody
                {
                    SessionToken = sessionToken,
                    TwoFactorCode = "000000"
                });
            
            secondResult.Should().BeOfType<ObjectResult>();
            var objectResult = secondResult as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }

    public class LogoutTests : LoginControllerTests
    {
        public LogoutTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Returns sign-out result")]
        public async Task ReturnsSignOut()
        {
            await AddMockUserAndLoginAsync();
            IActionResult logoutResult = await _controller.LogoutAsync(null);
            logoutResult.Should().BeOfType<SignOutResult>();
            _testOutputHelper.WriteLine("Result: " + logoutResult.GetType().Name);
        }
        
        [Fact(DisplayName = "Success: Logout when token provided as parameter")]
        public async Task ReturnsSignOut_WhenTokenProvided()
        {
            await AddMockUserAndLoginAsync();
            var setCookie = _controllerHttpContext.Response.Headers.SetCookie.ToString();
            setCookie.Should().Contain("mmc-token=");
            var cookiePair = setCookie.Split(';', 2)[0].Trim();
            var token = cookiePair.Split('=', 2)[1];
            
            IActionResult logoutResult = await _controller.LogoutAsync(token);
            logoutResult.Should().BeOfType<SignOutResult>();
        }

        [Fact(DisplayName = "Failure: Invalid token returns bad request")]
        public async Task ReturnsBadRequest_ForInvalidToken()
        {
            IActionResult result = await _controller.LogoutAsync("invalid-token");
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    private async Task<string?> AddMockUserAndLoginAsync(bool enableTwoFactor = false, bool performLogin = true)
    {
        _userMock.TwoFactorEnabled = enableTwoFactor;
        if (enableTwoFactor)
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
        
        await _dbContext.AddUserAsync(_userMock, true);

        if (!performLogin)
            return null;
        
        IActionResult result = await _controller.LoginAsync(new LoginRequestBody
        {
            Email = _userMock.Email,
            Password = _passwordMock
        });

        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult.Should().NotBeNull();
        
        var setCookie = _controllerHttpContext.Response.Headers.SetCookie.ToString();
        if (!string.IsNullOrEmpty(setCookie))
        {
            var cookiePair = setCookie.Split(';', 2)[0].Trim();
            _controllerHttpContext.Request.Headers.Cookie = cookiePair;
        }
        return contentResult.Content;
    }
    
    private async Task<string?> AddMockUserAndLoginLauncherAsync(bool enableTwoFactor = false, bool performLogin = true)
    {
        _userMock.TwoFactorEnabled = enableTwoFactor;
        if (enableTwoFactor)
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
        
        await _dbContext.AddUserAsync(_userMock, true);

        if (!performLogin)
            return null;
        
        IActionResult result = await _controller.LoginLauncherAsync(new LauncherLoginRequestBody
        {
            Username = _userMock.UserName,
            Password = _passwordMock
        });

        result.Should().BeOfType<ContentResult>();
        var contentResult = result as ContentResult;
        contentResult.Should().NotBeNull();
        
        var setCookie = _controllerHttpContext.Response.Headers.SetCookie.ToString();
        if (!string.IsNullOrEmpty(setCookie))
        {
            var cookiePair = setCookie.Split(';', 2)[0].Trim();
            _controllerHttpContext.Request.Headers.Cookie = cookiePair;
        }
        return contentResult.Content;
    }
}