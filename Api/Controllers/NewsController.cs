using Microsoft.AspNetCore.Mvc;

namespace Tavstal.MesterMC.Api.Controllers;

[ApiController]
[Route("/news")]
public class NewsController : CustomControllerBase
{
    protected NewsController(ILogger<NewsController> logger) : base(logger) { }
}