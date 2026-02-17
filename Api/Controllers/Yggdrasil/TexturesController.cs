using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

/// <summary>
/// Controller for handling Yggdrasil texture-related API requests.
/// </summary>
[ApiController]
[Route("yggdrasil/textures")]
[Tags("Yggdrasil")]
public class TexturesController : Controller
{
    private readonly ILogger _logger;
    private readonly CustomDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="TexturesController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="dbContext">The database context for accessing texture data.</param>
    public TexturesController(ILogger<TexturesController> logger, CustomDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }
    /// <summary>
    /// Retrieves texture data based on the provided hash.
    /// </summary>
    /// <param name="hash">The hash of the texture to retrieve.</param>
    /// <returns>
    /// A file response containing the texture data if found, or an appropriate error response.
    /// </returns>
    /// <response code="200">Returns the texture file.</response>
    /// <response code="404">Texture not found.</response>
    /// <response code="500">Failed to retrieve texture data.</response>
    [HttpGet("{hash}")]
    public async Task<IActionResult> GetTexture(string hash)
    {
        var fileData = await _dbContext.FindFileDataAsync(x => x.Hash == hash && (x.Type == EFileDataType.SKIN || x.Type == EFileDataType.CAPE));
        if (fileData == null)
          return this.ReturnResponseCode(HttpStatusCode.NotFound, "Texture not found.");
        byte[]? bytes = fileData.GetFileData();
        if (bytes == null)
          return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Failed to retrieve texture data.");
        return File(bytes, fileData.ContentType);
    }
}
