using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class SkinsControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SkinsControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}