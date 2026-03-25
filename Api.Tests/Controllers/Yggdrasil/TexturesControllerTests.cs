using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers.Yggdrasil;

public class TexturesControllerTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TexturesControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }
}