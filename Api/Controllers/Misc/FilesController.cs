using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    public FilesController(ILogger<FilesController> logger, CustomDbContext dbContext, MemoryCacheService memoryCache) : base(logger)
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
    public async Task<IActionResult> GetFile([BindRequired, FromRoute] string hash)
    {
        string cacheKey = $"file:{hash}";
        byte[]? bytes;
        string contentType;
        if (!_memoryCache.TryGetValue<(byte[], string)>(cacheKey, out var fd))
        {
            var fileData = await _dbContext.FindFileDataAsync(x => x.Hash == hash && IsPublicFileType(x.Type));
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
    
    /// <summary>
    /// Determines if the file type is public.
    /// </summary>
    /// <param name="type">The file type to check.</param>
    /// <returns>True if the file type is public; otherwise, false.</returns>
    private bool IsPublicFileType(EFileDataType type)
    {
        switch (type)
        {
            case EFileDataType.SKIN:
            case EFileDataType.CAPE:
            case EFileDataType.PROFILE_PICTURE:
            case EFileDataType.NEWS_BANNER:
                return true;
            default:
                return false;
        }
    }
}