using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Tavstal.MesterMC.Api.Controllers;

/// <summary>
/// Controller responsible for handling requests to the home endpoint.
/// </summary>
public class HomeController : CustomControllerBase
{
    protected HomeController(ILogger<HomeController> logger) : base(logger) { }
    
    /// <summary>
    /// Handles the root endpoint ("/") and returns an HTTP 200 OK response.
    /// This endpoint is ignored in the API documentation.
    /// </summary>
    /// <returns>An IActionResult representing the HTTP 200 OK response.</returns>
    [Route("/"), ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return ReturnResponseCode(HttpStatusCode.OK);
    }
}