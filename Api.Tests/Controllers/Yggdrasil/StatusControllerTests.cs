using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.Yggdrasil;
using Tavstal.MesterMC.Api.Services.Database;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Yggdrasil;

public class StatusControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<StatusController>> _loggerMock = new();
    private readonly StatusController _controller;
    
    public StatusControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new StatusController(_dbContext, (CustomUserManager)_userManager, _memoryCacheService, _loggerMock.Object, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }
}