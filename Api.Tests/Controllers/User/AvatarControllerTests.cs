using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class AvatarControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public AvatarControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}