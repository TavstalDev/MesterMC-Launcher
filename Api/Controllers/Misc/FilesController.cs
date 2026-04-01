using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Misc;

/// <summary>
/// Controller for managing file retrieval operations.
/// </summary>
[ApiController]
[Route("files")]
public class FilesController : CustomControllerBase
{
    private readonly CustomDbContext _dbContext;
    private readonly MemoryCacheService _memoryCache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(1);
    
    /// <summary>
    /// Initializes a new instance of the <see cref="FilesController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="dbContext">Database context for accessing file data.</param>
    /// <param name="memoryCache">Service for caching file data.</param>
    /// <param name="settings">Application settings.</param>
    public FilesController(ILogger<FilesController> logger, CustomDbContext dbContext, MemoryCacheService memoryCache, Settings settings) : base(logger, settings)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }
    
    /// <summary>
    /// Retrieves a file by its hash.
    /// </summary>
    /// <param name="hash">The hash of the file to retrieve.</param>
    /// <response code="200">File retrieved successfully.</response>
    /// <response code="304">File not modified since the last request.</response>
    /// <response code="404">File not found.</response>
    /// <response code="500">Failed to retrieve file data.</response>
    [HttpGet("{hash}")]
    [TextResponse(StatusCodes.Status200OK),
     TextResponse(StatusCodes.Status304NotModified),
     TextResponse(StatusCodes.Status404NotFound),
     TextResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetFile([BindRequired, FromRoute, MinLength(64), MaxLength(64)] string hash)
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
                    x.Hash == hash && (x.Type == EFileDataType.CAPE || x.Type == EFileDataType.SKIN ||
                                       x.Type == EFileDataType.PROFILE_PICTURE || x.Type == EFileDataType.NEWS_BANNER));
                if (fileData == null)
                    return ReturnResponseCode(HttpStatusCode.NotFound, "File not found.");
                bytes = fileData.GetFileData();
                if (bytes == null)
                    return ReturnResponseCode(HttpStatusCode.InternalServerError, "Failed to retrieve file data.");
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
            Logger.LogError(ex, "Error retrieving file with hash {Hash}", hash);
            return ReturnResponseCode(HttpStatusCode.InternalServerError, "An unknown error occurred while processing the request.");
        }
    }
}