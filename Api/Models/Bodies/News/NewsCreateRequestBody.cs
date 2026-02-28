namespace Tavstal.MesterMC.Api.Models.Bodies.News;

/// <summary>
/// Represents the request body for creating a news entry.
/// </summary>
public class NewsCreateRequestBody
{
    /// <summary>
    /// Gets or sets the title of the news entry.
    /// This field is required.
    /// </summary>
    public required string Title { get; set; }
        
    /// <summary>
    /// Gets or sets the content of the news entry.
    /// This field is required.
    /// </summary>
    public required string Content { get; set; }
    
    /// <summary>
    /// Gets or sets the banner image for the news entry.
    /// This field is required.
    /// </summary>
    public required IFormFile Banner { get; set; }
}
