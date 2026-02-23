namespace Tavstal.MesterMC.Api.Models.Bodies.News;

public class NewsCreateRequestBody
{
    public required string Title { get; set; }
    
    public required string Content { get; set; }
    
    public required IFormFile Banner { get; set; }
}