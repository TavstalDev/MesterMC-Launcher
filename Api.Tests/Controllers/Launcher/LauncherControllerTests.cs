using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.Launcher;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Models.Database.Launcher;
using Tavstal.MesterMC.Api.Services.Database;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Launcher;

public class LauncherControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<LauncherController>> _loggerMock = new();
    private readonly LauncherController _controller;

    public LauncherControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new LauncherController(_loggerMock.Object, (CustomUserManager)_userManager, _dbContext, _memoryCacheService, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }

    public class GetLauncherVersionTests : LauncherControllerTests
    {
        public GetLauncherVersionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Returns a list of versions")]
        public async Task ReturnsOk()
        {
            await _dbContext.AddLauncherVersionAsync(new LauncherVersion
            {
                Version = "1.0.0",
                Changelog = "Initial release",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, true);

            var result = await _controller.GetLauncherVersions();
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + contentResult.Content);
        }

        [Fact(DisplayName = "Failure: No version exists")]
        public async Task ReturnsNotFound()
        {
            var result = await _controller.GetLauncherVersions();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }

    public class GetLatestLauncherVersionTests : LauncherControllerTests
    {
        public GetLatestLauncherVersionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Returns the latest version")]
        public async Task ReturnsOk()
        {
            await _dbContext.AddLauncherVersionAsync(new LauncherVersion
            {
                Version = "1.0.0",
                Changelog = "Initial release",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, true);

            var result = await _controller.GetLatestLauncherVersion();
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + contentResult.Content);
        }

        [Fact(DisplayName = "Failure: No version exists")]
        public async Task ReturnsNotFound()
        {
            var result = await _controller.GetLatestLauncherVersion();
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }

    public class GetLauncherVersionDetailsTests : LauncherControllerTests
    {
        public GetLauncherVersionDetailsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }

        [Fact(DisplayName = "Success: Returns version details")]
        public async Task ReturnsOk()
        {
            var version = await _dbContext.AddLauncherVersionAsync(new LauncherVersion
            {
                Version = "1.0.0",
                Changelog = "Initial release",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, true);

            byte[] bytes = "Hello world"u8.ToArray();
            using var stream = new MemoryStream(bytes);
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            
            
            var fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = "test.zip",
                ContentType = "application/zip",
                Type = EFileDataType.LAUNCHER,
            }, true);
            
            await _dbContext.AddLauncherVersionDataAsync(new LauncherVersionData
            {
                VersionId = version.Id,
                FileId = fd.Id,
                Os = ELauncherOs.WINDOWS,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, true);
            
            var result = await _controller.GetLauncherVersionDetails(version.Id);
            result.Should().BeOfType<ContentResult>();
            var contentResult = result as ContentResult;
            contentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + contentResult.Content);
        }
        
        [Fact(DisplayName = "Failure: No version exists")]
        public async Task ReturnsNotFound()
        {
            var result = await _controller.GetLauncherVersionDetails(1);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }

    public class DownloadLauncherVersionTests : LauncherControllerTests
    {
        public DownloadLauncherVersionTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "Success: Returns file")]
        public async Task ReturnsOk()
        {
            var version = await _dbContext.AddLauncherVersionAsync(new LauncherVersion
            {
                Version = "1.0.0",
                Changelog = "Initial release",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, true);

            byte[] bytes = "Hello world"u8.ToArray();
            using var stream = new MemoryStream(bytes);
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            
            
            var fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = "test.zip",
                ContentType = "application/zip",
                Type = EFileDataType.LAUNCHER,
            }, true);
            fd.SaveFile(stream);
            await _dbContext.AddLauncherVersionDataAsync(new LauncherVersionData
            {
                VersionId = version.Id,
                FileId = fd.Id,
                Os = ELauncherOs.WINDOWS,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            }, true);
            
            var result = await _controller.DownloadLauncherVersion(version.Id, ELauncherOs.WINDOWS);
            result.Should().BeOfType<FileContentResult>();
            var fileContentResult = result as FileContentResult;
            fileContentResult.Should().NotBeNull();
            _testOutputHelper.WriteLine("Result: " + fileContentResult.ContentType);
            
            fd.DeleteFile();
        }

        [Fact(DisplayName = "Failure: No version exists")]
        public async Task ReturnsNotFound_WhenVersionDoesNotExist()
        {
            var result = await _controller.DownloadLauncherVersion(1, ELauncherOs.WINDOWS);
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult!.StatusCode.Should().Be(404);
            _testOutputHelper.WriteLine("Result: " + objectResult.Value);
        }
    }
}