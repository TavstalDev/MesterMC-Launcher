using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Tavstal.MesterMC.Api.Controllers.Launcher;
using Tavstal.MesterMC.Api.Controllers.User;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.User;
using Tavstal.MesterMC.Api.Services.Database;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class UserCapesControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<UserCapesController>> _loggerMock = new();
    private readonly UserCapesController _controller;
    
    public UserCapesControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new UserCapesController(_loggerMock.Object, (CustomUserManager)_userManager, _dbContext, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }

    public class SelectSkinTests : UserCapesControllerTests
    {
        public SelectSkinTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Select cape")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            var db = await FillDatabase(user.Id);
            var result = await _controller.SelectCape(db.cape.Id);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Cape already selected")]
        public async Task ReturnsBadRequest()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            var db = await FillDatabase(user.Id);
            await _controller.SelectCape(db.cape.Id);
            var result = await _controller.SelectCape(db.cape.Id);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Cape not found")]
        public async Task ReturnsNotFound()
        {
            await CreateUserAsync(_controller);
            var result = await _controller.SelectCape(1);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Unauthorized")]
        public async Task ReturnsUnauthorized()
        {
            var result = await _controller.SelectCape(1);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class ClearSelectedSkinTests : UserCapesControllerTests
    {
        public ClearSelectedSkinTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Clear selected cape")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            var db = await FillDatabase(user.Id);
            db.userCape.IsSelected = true;
            await _dbContext.UpdateUserCapeAsync(db.userCape, true);
            var result = await _controller.ClearSelectedCape();
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: No cape selected")]
        public async Task ReturnsBadRequest()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            var db = await FillDatabase(user.Id);
            var result = await _controller.ClearSelectedCape();
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Unauthorized")]
        public async Task ReturnsUnauthorized()
        {
            var result = await _controller.ClearSelectedCape();
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class SelectSkinAdminTests : UserCapesControllerTests
    {
        public SelectSkinAdminTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Select cape")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            var db = await FillDatabase(user.Id);
            var result = await _controller.SelectCapeAdmin(user.Id, db.cape.Id);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Cape already selected")]
        public async Task ReturnsBadRequest()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            var db = await FillDatabase(user.Id);
            await _controller.SelectCapeAdmin(user.Id, db.cape.Id);
            var result = await _controller.SelectCapeAdmin(user.Id, db.cape.Id);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Cape not found")]
        public async Task ReturnsNotFound()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            var result = await _controller.SelectCapeAdmin(user.Id, 1);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Not enough permissions")]
        public async Task ReturnsForbidden()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller, givePermissions: false);
            var result = await _controller.SelectCapeAdmin(user.Id, 1);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class ClearSelectedSkinAdminTests : UserCapesControllerTests
    {
        public ClearSelectedSkinAdminTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Clear selected cape")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            var db = await FillDatabase(user.Id);
            db.userCape.IsSelected = true;
            await _dbContext.UpdateUserCapeAsync(db.userCape, true);
            var result = await _controller.ClearSelectedCapeAdmin(user.Id);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: No cape selected")]
        public async Task ReturnsBadRequest()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            var db = await FillDatabase(user.Id);
            var result = await _controller.ClearSelectedCapeAdmin(user.Id);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Not enough permissions")]
        public async Task ReturnsForbidden()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller, givePermissions: false);
            var result = await _controller.ClearSelectedCapeAdmin(user.Id);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }

    private async Task<(FileData fileData, Cape cape, UserCape userCape)> FillDatabase(string userId)
    {
        using var stream = CreateTestImage(64, 64);
        using var sha256 = SHA256.Create();
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        string fileHash = Convert.ToHexStringLower(hashBytes);
            
        var fd = await _dbContext.AddFileDataAsync(new FileData
        {
            Hash = fileHash,
            FileName = "skin.png",
            ContentType = "image/png",
            UserId = userId,
            Type = EFileDataType.CAPE,
        }, true);
        fd.SaveFile(stream);
        var cape = await _dbContext.AddCapeAsync(new Cape
        {
            Name = "Test Cape",
            FileId = fd.Id,
            IsPublic = true
        }, true);
        var userCape = await _dbContext.AddUserCapeAsync(new UserCape
        {
            UserId = userId,
            CapeId = cape.Id,
            IsSelected = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt =  DateTime.UtcNow
        }, true);
        return (fd, cape, userCape);
    }
    
    /// <summary>
    /// Utility: creates and returns an in-memory PNG image stream of the specified width and height.
    /// The returned <see cref="MemoryStream"/> is positioned at 0 and ready for reading.
    /// </summary>
    /// <param name="width">Width in pixels for the generated image.</param>
    /// <param name="height">Height in pixels for the generated image.</param>
    /// <returns>A <see cref="MemoryStream"/> containing a PNG image.</returns>
    private MemoryStream CreateTestImage(int width, int height)
    {
        var stream = new MemoryStream();
        using (var image = new Image<Rgba32>(width, height))
        {
            // Save as PNG to the stream
            image.SaveAsPng(stream);
        }
        stream.Position = 0; // Reset for reading
        return stream;
    }
}