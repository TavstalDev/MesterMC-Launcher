using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Misc;

public class NewsControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public NewsControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}