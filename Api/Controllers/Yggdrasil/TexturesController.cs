using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Services.Database;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

[Route("yggdrasil/textures")]
public class TexturesController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;

    public TexturesController(IConfiguration configuration, ILogger<TexturesController> logger, CustomUserManager userManager, CustomDbContext dbContext)
    {
        _configuration = configuration;
        _logger = logger;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet("{hash}")]
    public async Task<IActionResult> GetTexture(string hash)
    {
        var fileData = await _dbContext.FindFileDataAsync(x => x.Hash == hash);
        if (fileData == null)
          return this.ReturnResponseCode(HttpStatusCode.NotFound, "Texture not found.");
        
        byte[]? bytes = fileData.GetFileData();
        if (bytes == null)
          return this.ReturnResponseCode(HttpStatusCode.InternalServerError, "Failed to retrieve texture data.");
        
        return File(bytes, fileData.ContentType);
    }
}