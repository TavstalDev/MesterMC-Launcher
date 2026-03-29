using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.User;
using Tavstal.MesterMC.Api.Services.Database;
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
}