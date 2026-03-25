using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Auth;

public class TwoFactorControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TwoFactorControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}