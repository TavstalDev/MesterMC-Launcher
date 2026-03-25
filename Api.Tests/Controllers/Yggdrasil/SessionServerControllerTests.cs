using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Yggdrasil;

public class SessionServerControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SessionServerControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}