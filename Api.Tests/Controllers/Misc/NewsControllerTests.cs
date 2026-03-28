using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Tavstal.MesterMC.Api.Controllers.Misc;
using Tavstal.MesterMC.Api.Models.Bodies.News;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Services.Database;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Misc;

public class NewsControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<NewsController>> _loggerMock = new();
    private readonly NewsController _controller;

    public NewsControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new NewsController(_loggerMock.Object, (CustomUserManager)_userManager, _dbContext, _memoryCacheService, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }

    public class GetNewsTests : NewsControllerTests
    {
        public GetNewsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        [Fact(DisplayName = "GetNews: Returns existing news with banner URL")]
        public async Task ReturnsOk()
        {
            await CreateUserAsync(_controller);

            using var stream = CreateTestImage(600, 100);
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            stream.Position = 0;

            FileData fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = $"{Guid.NewGuid():N}.png",
                ContentType = "image/png",
                Type = EFileDataType.NEWS_BANNER
            }, true);
            fd.SaveFile(stream);

            await _dbContext.AddNewsAsync(new News
            {
                Title = "Test Title",
                Content = "Test Content",
                BannerId = fd.Id,
                CreatedAt = DateTimeOffset.UtcNow
            }, true);
            
            var result = await _controller.GetNews();
            
            result.Should().BeOfType<ContentResult>();
            var contentResult = (ContentResult)result;
            var list = JsonConvert.DeserializeObject<List<NewsResponseBody>>(contentResult.Content);
            list.Should().NotBeNull();
            _testOutputHelper.WriteLine($"Response: {contentResult.Content}");
            
            var files = await _dbContext.GetFileDatasAsync();
            foreach (var f in files)
                f.DeleteFile();
        }
    }

    public class GetLatestNewsTests : NewsControllerTests
    {
        public GetLatestNewsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        
    }

    public class GetNewsByIdTests : NewsControllerTests
    {
        public GetNewsByIdTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        
    }

    public class CreateNewsTests : NewsControllerTests
    {
        public CreateNewsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        
    }
    
    public class UpdateNewsTests : NewsControllerTests
    {
        public UpdateNewsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        
    }
    
    public class DeleteNewsTests : NewsControllerTests
    {
        public DeleteNewsTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper) { }
        
        
    }
    
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