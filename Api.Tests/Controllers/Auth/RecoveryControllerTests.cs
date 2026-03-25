using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Auth;

public class RecoveryControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RecoveryControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}