using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.Yggdrasil;
using Tavstal.MesterMC.Api.Services.Database;
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
}