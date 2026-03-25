using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Yggdrasil;

/// <summary>
/// Controller for handling Yggdrasil texture-related API requests.
/// </summary>
[ApiController]
[Route("yggdrasil/textures")]
[Tags("Yggdrasil")]
public class TexturesController : CustomControllerBase
{
    private readonly CustomDbContext _dbContext;
    private readonly MemoryCacheService _memoryCache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(1);

    /// <summary>
    /// Initializes a new instance of the <see cref="TexturesController"/> class.
    /// </summary>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <param name="dbContext">The database context for accessing texture data.</param>
    /// <param name="memoryCache">The memory cache service for caching texture data.</param>
    public TexturesController(ILogger<TexturesController> logger, CustomDbContext dbContext, MemoryCacheService memoryCache) : base(logger)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }
    
    /// <summary>
    /// Retrieves texture data based on the provided hash.
    /// </summary>
    /// <param name="hash">The hash of the texture to retrieve.</param>
    /// <returns>
    /// A file response containing the texture data if found, or an appropriate error response.
    /// </returns>
    /// <response code="200">Returns the texture file.</response>
    /// <response code="304">Texture not modified (ETag matches).</response>
    /// <response code="404">Texture not found.</response>
    /// <response code="500">Failed to retrieve texture data.</response>
    [HttpGet("{hash}")]
    public async Task<IActionResult> GetTexture([BindRequired, FromRoute] string hash)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return ReturnResponseCode(HttpStatusCode.BadRequest,
                    string.IsNullOrEmpty(errorMessages) ? "Invalid input data." : errorMessages);
            }

            string cacheKey = $"file:{hash}";
            byte[]? bytes;
            string contentType;
            if (!_memoryCache.TryGetValue<(byte[], string)>(cacheKey, out var fd))
            {
                var fileData = await _dbContext.FindFileDataAsync(x =>
                    x.Hash == hash && (x.Type == EFileDataType.SKIN || x.Type == EFileDataType.CAPE));
                if (fileData == null)
                    return ReturnResponseCode(HttpStatusCode.NotFound, "Texture not found.");
                bytes = fileData.GetFileData();
                if (bytes == null)
                    return ReturnResponseCode(HttpStatusCode.InternalServerError, "Failed to retrieve texture data.");
                contentType = fileData.ContentType;
                _memoryCache.SetValue(cacheKey, (bytes, contentType), CacheTtl);
            }
            else
            {
                bytes = fd.Item1;
                contentType = fd.Item2;
            }

            string etag = "\"" + hash + "\"";
            if (HttpContext.Request.Headers.TryGetValue("If-None-Match", out var incomingEtag))
            {
                if (incomingEtag.ToString().Equals(etag, StringComparison.Ordinal))
                {
                    HttpContext.Response.Headers.ETag = etag;
                    HttpContext.Response.Headers.CacheControl = "public,max-age=86400,immutable";
                    return ReturnResponseCode(HttpStatusCode.NotModified);
                }
            }

            HttpContext.Response.Headers.ETag = etag;
            HttpContext.Response.Headers.CacheControl = "public,max-age=86400,immutable";
            return File(bytes, contentType);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving texture with hash {Hash}", hash);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
}
