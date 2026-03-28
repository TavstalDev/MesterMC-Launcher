using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tavstal.MesterMC.Api.Controllers;
using Tavstal.MesterMC.Api.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Tavstal.MesterMC.Api.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="HomeController"/>.
/// </summary>
public class HomeControllerTests
{
    /// <summary>
    /// Test output helper provided by xUnit. Use this to write diagnostics that appear in test logs.
    /// </summary>
    private readonly ITestOutputHelper _testOutputHelper;

    /// <summary>
    /// Creates a new instance of <see cref="HomeControllerTests"/>.
    /// </summary>
    /// <param name="testOutputHelper">
    /// xUnit-provided test output helper; useful for writing trace information for failing tests.
    /// </param>
    public HomeControllerTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    /// <summary>
    /// Verifies that the <see cref="HomeController.Index"/> action returns an HTTP 200 response
    /// with the message "The API is running." as the response value.
    /// </summary>
    [Fact(DisplayName = "Success: The API is running.")]
    public void Index_ReturnsOkWithMessage()
    {
        var loggerMock = new Mock<ILogger<HomeController>>();
        var controller = new HomeController(loggerMock.Object, TestHelper.CreateTestSettings());
        
        IActionResult result = controller.Index();
        
        result.Should().BeOfType<ObjectResult>();

        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(200);
        objectResult.Value.Should().Be("The API is running.");
        _testOutputHelper.WriteLine("Index action returned: {0}", objectResult.Value);
    }
}
