using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using SixLabors.ImageSharp;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Bodies.News;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Common;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Services;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Misc;

/// <summary>
/// Controller for managing news articles, including retrieving, creating, updating, and deleting news.
/// </summary>
[ApiController]
[Route("/news")]
public class NewsController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly Settings _settings;
    private readonly MemoryCacheService _cacheService;
    private readonly TimeSpan CacheTTL = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsController"/> class.
    /// </summary>
    /// <param name="logger">Logger instance for logging.</param>
    /// <param name="userManager">Custom user manager for user operations.</param>
    /// <param name="dbContext">Database context for accessing news data.</param>
    /// <param name="settings">Application settings.</param>
    /// <param name="cacheService">Service for caching news data.</param>
    public NewsController(ILogger<NewsController> logger, CustomUserManager userManager, CustomDbContext dbContext, Settings settings, MemoryCacheService cacheService) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _settings = settings;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Retrieves all news articles.
    /// </summary>
    /// <response code="200">Returns a list of all news articles.</response>
    [HttpGet]
    [JsonResponse(typeof(List<NewsResponseBody>))]
    public async Task<IActionResult> GetNews()
    {
        if (_cacheService.TryGetValue("news:all", out List<NewsResponseBody>? cachedNews) && cachedNews != null)
        {
            Response.Headers.CacheControl =
                "public,max-age=3600,immutable";
            return ReturnJson(cachedNews);
        }
        
        
        List<News> news = await _dbContext.GetNewsAsync();
        List<NewsResponseBody> newsResponse = [];
        foreach (News n in news)
        {
            string bannerUrl = string.Empty;
            FileData? fd = await _dbContext.FindFileDataAsync(x => x.Id == n.BannerId && x.Type == EFileDataType.NEWS_BANNER);
            if (fd != null)
                bannerUrl = fd.GetUrl(_settings.ApiUrl);
            newsResponse.Add(new NewsResponseBody
            {
                Title = n.Title,
                Content = n.Content,
                BannerUrl = bannerUrl
            });
        }
        
        _cacheService.SetValue("news:all", newsResponse, CacheTTL);
        Response.Headers.CacheControl =
            "public,max-age=3600,immutable";
        return ReturnJson(newsResponse);
    }
    
    /// <summary>
    /// Retrieves the latest news articles.
    /// </summary>
    /// <response code="200">Returns the latest news articles.</response>
    [HttpGet("latest")]
    [JsonResponse(typeof(List<NewsResponseBody>))]
    public async Task<IActionResult> GetLatestNews([FromQuery] int count = 5)
    {
        if (_cacheService.TryGetValue("news:latest", out List<NewsResponseBody>? cachedNews) && cachedNews != null)
        {
            Response.Headers.CacheControl =
                "public,max-age=3600,immutable";
            return ReturnJson(cachedNews);
        }
        
        List<News> news = await _dbContext.GetLatestNewsAsync(count);
        List<NewsResponseBody> newsResponse = [];
        foreach (News n in news)
        {
            string bannerUrl = string.Empty;
            FileData? fd = await _dbContext.FindFileDataAsync(x => x.Id == n.BannerId && x.Type == EFileDataType.NEWS_BANNER);
            if (fd != null)
                bannerUrl = fd.GetUrl(_settings.ApiUrl);
            newsResponse.Add(new NewsResponseBody
            {
                Title = n.Title,
                Content = n.Content,
                BannerUrl = bannerUrl
            });
        }
        
        _cacheService.SetValue("news:latest", newsResponse, CacheTTL);
        Response.Headers.CacheControl =
            "public,max-age=3600,immutable";
        return ReturnJson(newsResponse);
    }

    /// <summary>
    /// Retrieves a specific news article by its ID.
    /// </summary>
    /// <param name="id">The ID of the news article.</param>
    /// <response code="200">Returns the news article.</response>
    /// <response code="404">News article not found.</response>
    [HttpGet("{id}")]
    [JsonResponse(typeof(NewsResponseBody)), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNewsById([BindRequired, FromRoute] ulong id)
    {
        if (_cacheService.TryGetValue($"news:{id}", out NewsResponseBody? cachedNews) && cachedNews != null)
        {
            Response.Headers.CacheControl =
                "public,max-age=3600,immutable";
            return ReturnJson(cachedNews);
        }
        
        News? news = await _dbContext.FindNewsAsync(x => x.Id == id);
        if (news == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "News article not found.");

        string bannerUrl = string.Empty;
        FileData? fd = await _dbContext.FindFileDataAsync(x => x.Id == news.BannerId && x.Type == EFileDataType.NEWS_BANNER);
        if (fd != null)
            bannerUrl = fd.GetUrl(_settings.ApiUrl);

        var responseBody = new NewsResponseBody
        {
            Title = news.Title,
            Content = news.Content,
            BannerUrl = bannerUrl
        };
        _cacheService.SetValue($"news:{id}", responseBody, CacheTTL);
        Response.Headers.CacheControl =
            "public,max-age=3600,immutable";
        return ReturnJson(responseBody);
    }

    #region Admin endpoints
    /// <summary>
    /// Creates a new news article.
    /// </summary>
    /// <param name="requestBody">The request body containing news details.</param>
    /// <response code="200">News article created successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [Consumes("multipart/form-data")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateNews([FromForm] NewsCreateRequestBody requestBody)
    {
        var user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.News.Create))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");
        
        if (requestBody.Banner.Length > 1024 * 512) // 500 KB limit
            return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");
        
        if (!requestBody.Banner.FileName.EndsWith(".png"))
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

        await using var stream = requestBody.Banner.OpenReadStream();
        using var sha256 = SHA256.Create();
        byte[] hashBytes = await sha256.ComputeHashAsync(stream);
        string fileHash = Convert.ToHexStringLower(hashBytes);
        stream.Position = 0;
        
        try 
        {
            // Check Format and Dimensions using ImageSharp
            using var image = await Image.LoadAsync(stream);
            var info = image.Metadata.DecodedImageFormat;

            if (info?.Name != "PNG") 
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format (not a real PNG).");

            stream.Position = 0;
        }
        catch (Exception)
        {
            Logger.LogError($"Failed to upload banner file: {fileHash}");
            return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
        }

        FileData fd = await _dbContext.AddFileDataAsync(new FileData
        {
            Hash = fileHash,
            FileName = $"{Guid.NewGuid():N}.png",
            ContentType = "image/png",
            Type = EFileDataType.NEWS_BANNER
        }, true);
        fd.SaveFile(stream);

        await _dbContext.AddNewsAsync(new News
        {
            Title = requestBody.Title,
            Content = requestBody.Content,
            BannerId = fd.Id,
            CreatedAt = DateTimeOffset.UtcNow
        }, true);
        
        return ReturnResponseCode(HttpStatusCode.Created, "News article created successfully.");
    }

    /// <summary>
    /// Updates an existing news article.
    /// </summary>
    /// <param name="id">The ID of the news article to update.</param>
    /// <param name="requestBody">The request body containing updated news details.</param>
    /// <response code="200">News article updated successfully.</response>
    /// <response code="400">Invalid request.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="404">News article not found.</response>
    [HttpPut("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [Consumes("multipart/form-data")]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status400BadRequest), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNews([BindRequired, FromRoute] ulong id, [FromForm] NewsUpdateRequestBody requestBody)
    {
        var user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.News.Update))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

        News? news = await _dbContext.FindNewsAsync(x => x.Id == id);
        if (news == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "News article not found.");
        
        if (!string.IsNullOrEmpty(requestBody.Title))
            news.Title = requestBody.Title;
        
        if (!string.IsNullOrEmpty(requestBody.Content))
            news.Content = requestBody.Content;

        if (requestBody.Banner != null)
        {
            if (requestBody.Banner.Length > 1024 * 512) // 500 KB limit
                return ReturnResponseCode(HttpStatusCode.BadRequest, "File size exceeds the 500 KB limit.");
        
            if (!requestBody.Banner.FileName.EndsWith(".png"))
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Only PNG files are allowed.");

            await using var stream = requestBody.Banner.OpenReadStream();
            using var sha256 = SHA256.Create();
            byte[] hashBytes = await sha256.ComputeHashAsync(stream);
            string fileHash = Convert.ToHexStringLower(hashBytes);
            stream.Position = 0;
        
            try 
            {
                // Check Format and Dimensions using ImageSharp
                using var image = await Image.LoadAsync(stream);
                var info = image.Metadata.DecodedImageFormat;

                if (info?.Name != "PNG") 
                    return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format (not a real PNG).");

                stream.Position = 0;
            }
            catch (Exception)
            {
                Logger.LogError($"Failed to upload banner file: {fileHash}");
                return ReturnResponseCode(HttpStatusCode.BadRequest, "Invalid image format.");
            }
            
            FileData? existingBanner = await _dbContext.FindFileDataAsync(x => x.Id == news.BannerId && x.Type == EFileDataType.NEWS_BANNER);
            if (existingBanner != null)
            {
                existingBanner.DeleteFile();
                await _dbContext.RemoveFileDataAsync(existingBanner, true);
            }

            FileData fd = await _dbContext.AddFileDataAsync(new FileData
            {
                Hash = fileHash,
                FileName = $"{Guid.NewGuid():N}.png",
                ContentType = "image/png",
                Type = EFileDataType.NEWS_BANNER
            }, true);
            fd.SaveFile(stream);
            news.BannerId = fd.Id;
        }

        await _dbContext.UpdateNewsAsync(news, true);
        return ReturnResponseCode(HttpStatusCode.OK, "News article updated successfully.");
    }

    /// <summary>
    /// Deletes a news article by its ID.
    /// </summary>
    /// <param name="id">The ID of the news article to delete.</param>
    /// <response code="200">News article deleted successfully.</response>
    /// <response code="401">Unauthorized. User is not authenticated.</response>
    /// <response code="403">Forbidden. Insufficient permissions.</response>
    /// <response code="404">News article not found.</response>
    [HttpDelete("{id}")]
    [Authorize(AuthenticationSchemes = "Bearer,Basic")]
    [EnableRateLimiting(RateLimits.ADMIN)]
    [TextResponse(StatusCodes.Status200OK), TextResponse(StatusCodes.Status401Unauthorized), TextResponse(StatusCodes.Status403Forbidden), TextResponse(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNews([BindRequired, FromRoute] ulong id)
    {
        var user = await GetCurrentUserAsync(_userManager);
        if (user == null)
            return ReturnResponseCode(HttpStatusCode.Unauthorized, "User not authenticated");
        
        if (!_userManager.HasPermission(user, CustomPermissions.News.Delete))
            return ReturnResponseCode(HttpStatusCode.Forbidden, "You do not have enough permissions.");

        News? news = await _dbContext.FindNewsAsync(x => x.Id == id);
        if (news == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "News article not found.");

        FileData? file = await _dbContext.FindFileDataAsync(x => x.Id == news.BannerId && x.Type == EFileDataType.NEWS_BANNER);
        if (file != null)
        {
            file.DeleteFile();
            await _dbContext.RemoveFileDataAsync(file);
        }
        await _dbContext.RemoveNewsAsync(news, true);
        return ReturnResponseCode(HttpStatusCode.OK, "News article deleted successfully.");
    }
    #endregion
}