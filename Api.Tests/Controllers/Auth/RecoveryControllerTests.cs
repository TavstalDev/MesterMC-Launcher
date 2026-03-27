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
using Tavstal.MesterMC.Api.Utils.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Auth;

public class RecoveryControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly CustomDbContext _dbContext;
    private readonly UserManager<CustomUser> _userManager;
    private readonly RecoveryController _controller;
    private readonly DefaultHttpContext _controllerHttpContext;
    private readonly Settings _settings;
    private readonly CustomUser _userMock;
    private const string _passwordMock = "This%Valid_And#Pass%mock-2026";

    public RecoveryControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
        var loggerMock = new Mock<ILogger<RecoveryController>>();
        _dbContext = TestHelper.CreateInMemoryDbContext();
        var userManager = TestHelper.CreateCustomUserManager(_dbContext);
        _userManager = userManager;
        var emailService = TestHelper.FakeEmailService;
        _settings = TestHelper.CreateTestSettings();
        var memoryService = TestHelper.MemoryCacheService;
        _controller = new RecoveryController(loggerMock.Object, userManager, _dbContext, emailService, memoryService, _settings);

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
            PasswordHash = StringChiper.GetEncryptedSha256Hash(_passwordMock, _settings.EncryptionKey),
            CreateDate = DateTimeOffset.UtcNow,
            LastLogin = DateTimeOffset.UtcNow,
            LastUpdate = DateTimeOffset.UtcNow,
            SkinModel = ESkinType.WIDE
        };
    }

    public class RequestRecoveryTests : RecoveryControllerTests
    {
        public RequestRecoveryTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Send recovery email")]
        public async Task ReturnsOk()
        {
            _userMock.EmailConfirmed = true;
            await _userManager.CreateAsync(_userMock, _passwordMock);
            IActionResult result = await _controller.RequestRecoveryAsync(_userMock.Email);

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(201);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: User not found")]
        public async Task ReturnsNotFound_WhenUserMissing()
        {
            IActionResult result = await _controller.RequestRecoveryAsync("noone@example.com");
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Email not confirmed")]
        public async Task ReturnsForbidden_WhenEmailNotConfirmed()
        {
            _userMock.EmailConfirmed = false;
            await _userManager.CreateAsync(_userMock, _passwordMock);

            IActionResult result = await _controller.RequestRecoveryAsync(_userMock.Email);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Already requested recovery recently")]
        public async Task ReturnsForbidden_WhenRequestTooFrequent()
        {
            await _userManager.CreateAsync(_userMock, _passwordMock);
            string userId = _userManager.Users.First().Id;
            var memoryService = TestHelper.MemoryCacheService;
            string fingerprint = TestHelper.GetFingerprint(userId);
            string cacheKey = $"recovery:{fingerprint}:password:token";
            memoryService.SetValue(cacheKey, "existing-token", TimeSpan.FromMinutes(15));

            IActionResult result = await _controller.RequestRecoveryAsync(_userMock.Email);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }

    public class RecoverPasswordTests : RecoveryControllerTests
    {
        public RecoverPasswordTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Reset password")]
        public async Task ReturnsOk()
        {
            await _userManager.CreateAsync(_userMock, _passwordMock);
            string token = TokenHelper.GenerateRecoveryToken();
            string fingerprint = TestHelper.GetFingerprint(_userMock.Id);
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:password:token", token, TimeSpan.FromMinutes(15));
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:password:attempt", 0, TimeSpan.FromMinutes(15));

            IActionResult result = await _controller.RecoverPasswordAsync(new RecoverPasswordRequestBody
            {
                Email = _userMock.Email,
                RecoveryToken = token,
                NewPassword = "NewPass-dasPW#2026",
                LogoutEverywhere = true
            });

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Invalid recovery token")]
        public async Task ReturnsUnauthorized_ForInvalidToken()
        {
            await _userManager.CreateAsync(_userMock, _passwordMock);
            IActionResult result = await _controller.RecoverPasswordAsync(new RecoverPasswordRequestBody
            {
                Email = _userMock.Email,
                RecoveryToken = "wrong-token",
                NewPassword = "NewPass-dasPW#2026",
                LogoutEverywhere = false
            });

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Expired recovery token")]
        public async Task ReturnsBadRequest_WhenTokenExpired()
        {
            await _userManager.CreateAsync(_userMock, _passwordMock);
            string token = TokenHelper.GenerateRecoveryToken();
            
            IActionResult result = await _controller.RecoverPasswordAsync(new RecoverPasswordRequestBody
            {
                Email = _userMock.Email,
                RecoveryToken = token,
                NewPassword = "NewPass-dasPW#2026",
                LogoutEverywhere = false
            });

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Too many attempts")]
        public async Task ReturnsForbidden_WhenTooManyAttempts()
        {
            await _userManager.CreateAsync(_userMock, _passwordMock);
            string token = TokenHelper.GenerateRecoveryToken();
            string fingerprint = TestHelper.GetFingerprint(_userMock.Id);
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:password:token", token, TimeSpan.FromMinutes(15));
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:password:attempt", 4, TimeSpan.FromMinutes(15));
            
            IActionResult result = await _controller.RecoverPasswordAsync(new RecoverPasswordRequestBody
            {
                Email = _userMock.Email,
                RecoveryToken = token,
                NewPassword = "NewPass-dasPW#2026",
                LogoutEverywhere = false
            });

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }

    public class RequestTwoFactorTests : RecoveryControllerTests
    {
        public RequestTwoFactorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) {}
        
        [Fact(DisplayName = "Success: Send recovery email")]
        public async Task ReturnsOk()
        {
            _userMock.EmailConfirmed = true;
            await _userManager.CreateAsync(_userMock, _passwordMock);
            IActionResult result = await _controller.RequestTFARecoveryAsync(_userMock.Email);

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(201);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: User not found")]
        public async Task ReturnsNotFound_WhenUserMissing()
        {
            IActionResult result = await _controller.RequestTFARecoveryAsync("noone@example.com");
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Email not confirmed")]
        public async Task ReturnsForbidden_WhenEmailNotConfirmed()
        {
            _userMock.EmailConfirmed = false;
            await _userManager.CreateAsync(_userMock, _passwordMock);

            IActionResult result = await _controller.RequestTFARecoveryAsync(_userMock.Email);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Already requested recovery recently")]
        public async Task ReturnsForbidden_WhenRequestTooFrequent()
        {
            await _userManager.CreateAsync(_userMock, _passwordMock);
            string userId = _userManager.Users.First().Id;
            var memoryService = TestHelper.MemoryCacheService;
            string fingerprint = TestHelper.GetFingerprint(userId);
            string cacheKey = $"recovery:{fingerprint}:tfa:token";
            memoryService.SetValue(cacheKey, "existing-token", TimeSpan.FromMinutes(15));

            IActionResult result = await _controller.RequestTFARecoveryAsync(_userMock.Email);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class RecoverTwoFactorTests : RecoveryControllerTests
    {
        public RecoverTwoFactorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Recover 2FA with valid backup code")]
        public async Task ReturnsOk_WhenValidBackupCode()
        {
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _userManager.CreateAsync(_userMock);
            var user = _userManager.Users.First();
            string backup = "backup-code-123";
            await _dbContext.AddUserBackupCodeAsync(new UserBackupCode
            {
                UserId = user.Id,
                HashedCode = StringChiper.GetEncryptedSha256Hash(backup, _settings.EncryptionKey),
                CreateAt =  DateTime.UtcNow,
            }, true);
            string fingerprint = TestHelper.GetFingerprint(_userMock.Id);
            string token = TokenHelper.GenerateRecoveryToken();
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:tfa:token", token, TimeSpan.FromMinutes(15));
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:tfa:attempt", 0, TimeSpan.FromMinutes(15));

            IActionResult result = await _controller.RecoverTwoFactorAsync(new RecoverTwoFactorRequestBody
            {
                Email = _userMock.Email,
                BackupCode = backup,
                RecoveryToken = token,
                LogoutEverywhere = true
            });

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Invalid backup code")]
        public async Task ReturnsUnauthorized_ForInvalidBackupCode()
        {
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _userManager.CreateAsync(_userMock);

            string fingerprint = TestHelper.GetFingerprint(_userMock.Id);
            string token = TokenHelper.GenerateRecoveryToken();
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:tfa:token", token, TimeSpan.FromMinutes(15));
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:tfa:attempt", 0, TimeSpan.FromMinutes(15));

            IActionResult result = await _controller.RecoverTwoFactorAsync(new RecoverTwoFactorRequestBody
            {
                Email = _userMock.Email,
                BackupCode = "wrong-backup",
                RecoveryToken = token,
                LogoutEverywhere = false
            });

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: Too many recovery attempts")]
        public async Task ReturnsForbidden_WhenTooManyAttempts()
        {
            _userMock.TwoFactorEnabled = true;
            _userMock.TwoFactorSecret = TokenHelper.GenerateTwoFactorToken();
            await _userManager.CreateAsync(_userMock);
            
            string fingerprint = TestHelper.GetFingerprint(_userMock.Id);
            string token = TokenHelper.GenerateRecoveryToken();
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:tfa:token", token, TimeSpan.FromMinutes(15));
            TestHelper.MemoryCacheService.SetValue($"recovery:{fingerprint}:tfa:attempt", 4, TimeSpan.FromMinutes(15));

            IActionResult result = await _controller.RecoverTwoFactorAsync(new RecoverTwoFactorRequestBody
            {
                Email = _userMock.Email,
                RecoveryToken = "",
                BackupCode = "anything",
                LogoutEverywhere = false
            });

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: User not found")]
        public async Task ReturnsNotFound_WhenUserMissing()
        {
            IActionResult result = await _controller.RecoverTwoFactorAsync(new RecoverTwoFactorRequestBody
            {
                Email = "noone@example.com",
                RecoveryToken = "",
                BackupCode = "any",
                LogoutEverywhere = false
            });

            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
}

