namespace Tavstal.MesterMC.Api.Models.Bodies.News;

/// <summary>
/// Represents the request body for updating an existing news entry.
/// </summary>
public class NewsUpdateRequestBody
{
    /// <summary>
    /// Gets or sets the title of the news entry.
    /// This field is optional.
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Gets or sets the content of the news entry.
    /// This field is optional.
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// Gets or sets the banner image for the news entry.
    /// This field is optional.
    /// </summary>
    public IFormFile? Banner { get; set; }
}
