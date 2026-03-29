using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.Launcher;
using Tavstal.MesterMC.Api.Controllers.User;
using Tavstal.MesterMC.Api.Services.Database;
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
}