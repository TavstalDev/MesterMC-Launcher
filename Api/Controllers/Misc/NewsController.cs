using System.Net;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using SixLabors.ImageSharp;
using Tavstal.MesterMC.Api.Models;
using Tavstal.MesterMC.Api.Models.Bodies.News;
using Tavstal.MesterMC.Api.Models.Claims;
using Tavstal.MesterMC.Api.Models.Database;
using Tavstal.MesterMC.Api.Services.Database;

namespace Tavstal.MesterMC.Api.Controllers.Misc;

[ApiController]
[Route("/news")]
public class NewsController : CustomControllerBase
{
    private readonly CustomUserManager _userManager;
    private readonly CustomDbContext _dbContext;
    private readonly Settings _settings;

    protected NewsController(ILogger<NewsController> logger, CustomUserManager userManager, CustomDbContext dbContext, Settings settings) : base(logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _settings = settings;
    }

    [HttpGet]
    public async Task<IActionResult> GetNews()
    {
        List<News> news = await _dbContext.GetNewsAsync();
        List<NewsResponseBody> newsResponse = new List<NewsResponseBody>();
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
        
        Response.Headers.CacheControl =
            "public,max-age=3600,immutable";
        return ReturnJson(newsResponse);
    }
    
    [HttpGet("latest")]
    public async Task<IActionResult> GetLatestNews()
    {
        List<News> news = await _dbContext.GetLatestNewsAsync(5);
        List<NewsResponseBody> newsResponse = new List<NewsResponseBody>();
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
        
        Response.Headers.CacheControl =
            "public,max-age=3600,immutable";
        return ReturnJson(newsResponse);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetNewsById([BindRequired, FromRoute] ulong id)
    {
        News? news = await _dbContext.FindNewsAsync(x => x.Id == id);
        if (news == null)
            return ReturnResponseCode(HttpStatusCode.NotFound, "News article not found.");

        string bannerUrl = string.Empty;
        FileData? fd = await _dbContext.FindFileDataAsync(x => x.Id == news.BannerId && x.Type == EFileDataType.NEWS_BANNER);
        if (fd != null)
            bannerUrl = fd.GetUrl(_settings.ApiUrl);
        
        Response.Headers.CacheControl =
            "public,max-age=3600,immutable";
        return ReturnJson(new NewsResponseBody
        {
            Title = news.Title,
            Content = news.Content,
            BannerUrl = bannerUrl
        });
    }

    #region Admin endpoints

    [HttpPost]
    public async Task<IActionResult> CreateNews([BindRequired, FromBody] NewsCreateRequestBody requestBody)
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
        string fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
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


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNews([BindRequired, FromRoute] ulong id, [BindRequired, FromBody] NewsUpdateRequestBody requestBody)
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
            string fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
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

    [HttpDelete("{id}")]
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