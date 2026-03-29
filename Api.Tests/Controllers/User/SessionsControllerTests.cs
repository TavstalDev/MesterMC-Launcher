using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.User;
using Tavstal.MesterMC.Api.Services.Database;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class SessionsControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<SessionsController>> _loggerMock = new();
    private readonly SessionsController _controller;
    
    public SessionsControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new SessionsController(_loggerMock.Object, (CustomUserManager)_userManager, _dbContext, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }
}