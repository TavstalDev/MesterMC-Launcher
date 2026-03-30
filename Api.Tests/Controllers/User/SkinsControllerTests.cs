using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Tavstal.MesterMC.Api.Controllers.User;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Services.Database;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class SkinsControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<SkinsController>> _loggerMock = new();
    private readonly SkinsController _controller;
    
    public SkinsControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new SkinsController(_loggerMock.Object, (CustomUserManager)_userManager, _dbContext, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }
    
    public class GetSkinTests : SkinsControllerTests
    {
        public GetSkinTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Get existing skin")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            using var stream = CreateTestImage(64, 64);
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            
            var fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = "skin.png",
                ContentType = "image/png",
                UserId = user.Id,
                Type = EFileDataType.SKIN,
            }, true);
            fd.SaveFile(stream);
            var result = await _controller.GetSkin();
            
            result.Should().BeOfType<FileStreamResult>();
            var fileStreamResult = result as FileStreamResult;
            fileStreamResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + fileStreamResult.ContentType);
            
            fd.DeleteFile();
        }
        
        [Fact(DisplayName = "Failure: Unauthorized")]
        public async Task ReturnsUnauthorized()
        {
            var result = await _controller.GetSkin();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: No skin found")]
        public async Task ReturnsNotFound()
        {
            await CreateUserAsync(_controller);
            var result = await _controller.GetSkin();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class UploadSkinTests : SkinsControllerTests
    {
        public UploadSkinTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Upload skin")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);
            using var stream = CreateTestImage(64, 64);
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            
            var result = await _controller.UploadSkin(file);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);

            var files = await _dbContext.GetFileDatasAsync();
            foreach (var f in files)
                f.DeleteFile();
        }
        
        [Fact(DisplayName = "Failure: Unauthorized")]
        public async Task ReturnsUnauthorized()
        {
            using var stream = CreateTestImage(64, 64);
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            var result = await _controller.UploadSkin(file);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: File too large")]
        public async Task ReturnsBadRequest_WhenFileTooLarge()
        {
            await CreateUserAsync(_controller);
            using var stream = new MemoryStream(new byte[1024 * 512]);
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            var result = await _controller.UploadSkin(file);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Invalid dimensions")]
        public async Task ReturnsBadRequest_WhenInvalidDimensions()
        {
            await CreateUserAsync(_controller);
            using var stream = CreateTestImage(64, 65);
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            var result = await _controller.UploadSkin(file);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class DeleteSkinTests : SkinsControllerTests
    {
        public DeleteSkinTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Delete skin")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);
            var user = _dbContext.Users.First();
            using var stream = CreateTestImage(64, 64);
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            
            var fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = "skin.png",
                ContentType = "image/png",
                UserId = user.Id,
                Type = EFileDataType.SKIN,
            }, true);
            fd.SaveFile(stream);
            var result = await _controller.DeleteSkin();
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
            
            fd.DeleteFile();
        }
        
        [Fact(DisplayName = "Failure: Unauthorized")]
        public async Task ReturnsUnauthorized()
        {
            var result = await _controller.DeleteSkin();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(401);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: No skin found")]
        public async Task ReturnsNotFound()
        {
            await CreateUserAsync(_controller);
            var result = await _controller.DeleteSkin();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class GetSkinAdminTests : SkinsControllerTests
    {
        public GetSkinAdminTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Get existing skin")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            using var stream = CreateTestImage(64, 64);
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            
            var fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = "skin.png",
                ContentType = "image/png",
                UserId = user.Id,
                Type = EFileDataType.SKIN,
            }, true);
            fd.SaveFile(stream);
            var result = await _controller.GetSkinAdmin(user.Id);
            
            result.Should().BeOfType<FileStreamResult>();
            var fileStreamResult = result as FileStreamResult;
            fileStreamResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + fileStreamResult.ContentType);
            
            fd.DeleteFile();
        }
        
        [Fact(DisplayName = "Failure: No permissions")]
        public async Task ReturnsUnauthorized()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller, givePermissions: false);
            var result = await _controller.GetSkinAdmin(user.Id);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: No skin found")]
        public async Task ReturnsNotFound()
        {
            await CreateUserAsync(_controller, _userMock2, false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            var result = await _controller.GetSkinAdmin(user.Id);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class UploadSkinAdminTests : SkinsControllerTests
    {
        public UploadSkinAdminTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Upload skin")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller, _userMock2, givePermissions: false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            using var stream = CreateTestImage(64, 64);
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            
            var result = await _controller.UploadSkinAdmin(user.Id, file);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);

            var files = await _dbContext.GetFileDatasAsync();
            foreach (var f in files)
                f.DeleteFile();
        }
        
        [Fact(DisplayName = "Failure: Not enough permissions")]
        public async Task ReturnsForbidden()
        {
            await CreateUserAsync(_controller, _userMock2, givePermissions: false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller, givePermissions: false);
            using var stream = CreateTestImage(64, 64);
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            var result = await _controller.UploadSkinAdmin(user.Id, file);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: File too large")]
        public async Task ReturnsBadRequest_WhenFileTooLarge()
        {
            await CreateUserAsync(_controller, _userMock2, givePermissions: false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            using var stream = new MemoryStream(new byte[1024 * 512]);
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            var result = await _controller.UploadSkinAdmin(user.Id, file);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
        
        [Fact(DisplayName = "Failure: Invalid dimensions")]
        public async Task ReturnsBadRequest_WhenInvalidDimensions()
        {
            await CreateUserAsync(_controller, _userMock2, givePermissions: false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            using var stream = CreateTestImage(64, 65);
            IFormFile file = new FormFile(stream, 0, stream.Length, "file", "test.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            var result = await _controller.UploadSkinAdmin(user.Id, file);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(400);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
    
    public class DeleteSkinAdminTests : SkinsControllerTests
    {
        public DeleteSkinAdminTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Delete skin")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller, _userMock2, givePermissions: false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            using var stream = CreateTestImage(64, 64);
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            
            var fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = "skin.png",
                ContentType = "image/png",
                UserId = user.Id,
                Type = EFileDataType.SKIN,
            }, true);
            fd.SaveFile(stream);
            var result = await _controller.DeleteSkinAdmin(user.Id);
            
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(200);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
            
            fd.DeleteFile();
        }
        
        [Fact(DisplayName = "Failure: Not enough permissions")]
        public async Task ReturnsForbidden()
        {
            await CreateUserAsync(_controller, _userMock2, givePermissions: false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller, givePermissions: false);
            var result = await _controller.DeleteSkinAdmin(user.Id);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(403);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }

        [Fact(DisplayName = "Failure: No skin found")]
        public async Task ReturnsNotFound()
        {
            await CreateUserAsync(_controller, _userMock2, givePermissions: false);
            var user = _dbContext.Users.First();
            await CreateUserAsync(_controller);
            var result = await _controller.DeleteSkinAdmin(user.Id);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
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