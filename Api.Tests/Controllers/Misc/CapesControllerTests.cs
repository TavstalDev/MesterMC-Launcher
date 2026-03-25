using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Misc;

public class CapesControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public CapesControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}