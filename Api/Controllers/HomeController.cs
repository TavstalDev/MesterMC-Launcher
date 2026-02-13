using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers;

/// <summary>
/// Controller responsible for handling requests to the home endpoint.
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// Handles the root endpoint ("/") and returns an HTTP 200 OK response.
    /// This endpoint is ignored in the API documentation.
    /// </summary>
    /// <returns>An IActionResult representing the HTTP 200 OK response.</returns>
    [Route("/"), ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return this.ReturnResponseCode(HttpStatusCode.OK);
    }
}