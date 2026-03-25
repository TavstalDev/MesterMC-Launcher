using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class SessionsControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SessionsControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}