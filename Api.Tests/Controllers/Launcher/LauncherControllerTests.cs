using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.Launcher;
using Tavstal.MesterMC.Api.Services.Database;
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
}