using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Misc;

public class FilesControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public FilesControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}