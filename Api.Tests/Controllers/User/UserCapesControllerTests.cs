using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class UserCapesControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public UserCapesControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}