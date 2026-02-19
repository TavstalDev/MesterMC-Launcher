using Microsoft.AspNetCore.Mvc;

namespace Tavstal.MesterMC.Api.Controllers.Misc;

[ApiController]
[Route("/news")]
public class NewsController : CustomControllerBase
{
    // TODO: Add following endpoints:
    // - GET /news: Get a list of news articles.
    // - GET /news/{id}: Get a specific news article by its ID.
    // - POST /news: Create a new news article (admin only).
    // - PUT /news/{id}: Update a specific news article by its ID (admin only).
    // - DELETE /news/{id}: Delete a specific news article by its ID (admin only).
    
    protected NewsController(ILogger<NewsController> logger) : base(logger) { }
}