using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.Yggdrasil;
using Tavstal.MesterMC.Api.Models.Bodies.Yggdrasil;
using Tavstal.MesterMC.Api.Models.Database.Server;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Tests.Helpers;
using Tavstal.MesterMC.Api.Utils.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Yggdrasil;

public class SessionServerControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<SessionServerController>> _loggerMock = new();
    private readonly SessionServerController _controller;
    
    public SessionServerControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new SessionServerController(_loggerMock.Object, (CustomUserManager)_userManager, _dbContext, _memoryCacheService, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }
    
    public class BlockedServersTests : SessionServerControllerTests
    {
        public BlockedServersTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: List of blocked servers")]
        public async Task ReturnsOk()
        {
            var result = _controller.GetBlockedServers();
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + contentResult.Content);
        }
    }
    
    public class JoinTests : SessionServerControllerTests
    {
        public JoinTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Successful join")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            _controllerHttpContext.HttpContext.Request.Host = new HostString(TestHelper.IpAddress);
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            var result = await _controller.Join(new YigJoinServerRequest
            {
                accessToken = userPlaySession.Token,
                selectedProfile = user.Id,
                serverId = Guid.NewGuid().ToString()
            });
            
            result.Should().BeOfType<StatusCodeResult>();
            var statusCodeResult = result as StatusCodeResult;
            statusCodeResult!.StatusCode.Should().Be(204);
        }

        [Fact(DisplayName = "Failure: Invalid uuid")]
        public async Task ReturnsNotFound_WhenInvalidUuid()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            _controllerHttpContext.HttpContext.Request.Host = new HostString(TestHelper.IpAddress);
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            var result = await _controller.Join(new YigJoinServerRequest
            {
                accessToken = userPlaySession.Token,
                selectedProfile = Guid.NewGuid().ToString(),
                serverId = Guid.NewGuid().ToString()
            });
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failed: Invalid ip")]
        public async Task ReturnsForbidden_WhenIpDoesNotMatch()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            _controllerHttpContext.HttpContext.Request.Host = new HostString("192.168.0.1");
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            var result = await _controller.Join(new YigJoinServerRequest
            {
                accessToken = userPlaySession.Token,
                selectedProfile = user.Id,
                serverId = Guid.NewGuid().ToString()
            });
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failed: Expired session")]
        public async Task ReturnsUnauthorized_WhenExpiredSession()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            _controllerHttpContext.HttpContext.Request.Host = new HostString(TestHelper.IpAddress);
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-30),
            }, true);
            var result = await _controller.Join(new YigJoinServerRequest
            {
                accessToken = userPlaySession.Token,
                selectedProfile = user.Id,
                serverId = Guid.NewGuid().ToString()
            });
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class HasJoinedTests : SessionServerControllerTests
    {
        public HasJoinedTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: User has joined")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            _controllerHttpContext.HttpContext.Request.Host = new HostString(TestHelper.IpAddress);
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            string serverId = Guid.NewGuid().ToString();
            await _dbContext.AddServerJoinAsync(new ServerJoin
            {
                ServerId = serverId,
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                CreatedAt =  DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            
            var result = await _controller.HasJoined(serverId, user.UserName, TestHelper.IpAddress);
            
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + contentResult.Content);
        }
        
        [Fact(DisplayName = "Failure: Join does not exist")]
        public async Task ReturnsNotFound_WhenJoinDoesNotExist()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            _controllerHttpContext.HttpContext.Request.Host = new HostString(TestHelper.IpAddress);
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            string serverId = Guid.NewGuid().ToString();
            
            var result = await _controller.HasJoined(serverId, user.UserName, TestHelper.IpAddress);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Join expired")]
        public async Task ReturnsUnauthorized_WhenJoinExpired()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            _controllerHttpContext.HttpContext.Request.Host = new HostString(TestHelper.IpAddress);
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            string serverId = Guid.NewGuid().ToString();
            await _dbContext.AddServerJoinAsync(new ServerJoin
            {
                ServerId = serverId,
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                CreatedAt =  DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-30),
            }, true);
            
            var result = await _controller.HasJoined(serverId, user.UserName, TestHelper.IpAddress);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: User id does not match")]
        public async Task ReturnsBadRequest_WhenUserIdDoesNotMatch()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            var admin = _dbContext.Users.First(u => u.Id != user.Id);
            _controllerHttpContext.HttpContext.Request.Host = new HostString(TestHelper.IpAddress);
            var userPlaySession = await _dbContext.AddUserPlaySessionAsync(new UserPlaySession
            {
                UserId = user.Id,
                UserIp = TestHelper.IpAddress,
                Token = TokenHelper.GenerateToken(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            string serverId = Guid.NewGuid().ToString();
            await _dbContext.AddServerJoinAsync(new ServerJoin
            {
                ServerId = serverId,
                UserId = admin.Id,
                UserIp = TestHelper.IpAddress,
                CreatedAt =  DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            }, true);
            
            var result = await _controller.HasJoined(serverId, user.UserName, TestHelper.IpAddress);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class GetProfileTests : SessionServerControllerTests
    {
        public GetProfileTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Get profile by uuid")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            var result = await _controller.GetProfile(user.Id);
            
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + contentResult.Content);
        }
        
        [Fact(DisplayName = "Failure: User does not exist")]
        public async Task ReturnsNotFound()
        {
            var result = await _controller.GetProfile(Guid.NewGuid().ToString());
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
}