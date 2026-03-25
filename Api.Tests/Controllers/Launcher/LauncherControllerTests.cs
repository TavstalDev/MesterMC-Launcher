using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Launcher;

public class LauncherControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public LauncherControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}