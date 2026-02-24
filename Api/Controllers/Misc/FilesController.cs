using System.Net;
using Microsoft.AspNetCore.Mvc;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Misc;

[ApiController]
[Route("files")]
public class FilesController : CustomControllerBase
{
    private readonly CustomDbContext _dbContext;
    private readonly MemoryCacheService _memoryCache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromDays(1);
    
    public FilesController(ILogger<FilesController> logger, CustomDbContext dbContext, MemoryCacheService memoryCache) : base(logger)
    {
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }
    
    [HttpGet("{hash}")]
    public async Task<IActionResult> GetFile([FromRoute] string hash)
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