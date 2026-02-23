namespace Tavstal.MesterMC.Api.Models.Bodies.News;

public class NewsUpdateRequestBody
{
    public string? Title { get; set; }
    
    public string? Content { get; set; }
    
    public IFormFile? Banner { get; set; }
}