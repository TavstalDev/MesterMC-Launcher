using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers.Yggdrasil;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Yggdrasil;

public class ProfilesControllerTests : ControllerTestBase
{
    private readonly Mock<ILogger<ProfilesController>> _loggerMock = new();
    private readonly ProfilesController _controller;
    
    public ProfilesControllerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _controller = new ProfilesController(_loggerMock.Object, _dbContext, _settings);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _controllerHttpContext
        };
    }
}