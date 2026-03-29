using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.Yggdrasil;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Yggdrasil;

public class TexturesControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<TexturesController>> _loggerMock = new();
    private readonly TexturesController _controller;
    
    public TexturesControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new TexturesController(_loggerMock.Object, _dbContext, _memoryCacheService, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }
}