using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.User;

public class PublicUserControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PublicUserControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}