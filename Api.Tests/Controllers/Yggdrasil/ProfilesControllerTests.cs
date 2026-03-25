using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Yggdrasil;

public class ProfilesControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ProfilesControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}