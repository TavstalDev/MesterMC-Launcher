using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Yggdrasil;

public class StatusControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public StatusControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}