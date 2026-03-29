using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.User;
using Tavstal.MesterMC.Api.Services.Database;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class AvatarControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<AvatarController>> _loggerMock = new();
    private readonly AvatarController _controller;
    
    public AvatarControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new AvatarController(_loggerMock.Object, (CustomUserManager)_userManager, _dbContext, _memoryCacheService, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }
}