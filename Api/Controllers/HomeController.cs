using Microsoft.AspNetCore.Mvc;

namespace Tavstal.MesterMC.Api.Controllers;

public class HomeController : Controller
{
    [Route("/"), ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Index()
    {
        return Ok();
    }
}