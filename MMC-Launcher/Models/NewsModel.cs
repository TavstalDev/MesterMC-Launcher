using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.MesterMC.Launcher.Models;

/// <summary>
/// Represents a model for news items, containing the title, content, and an optional banner image.
/// </summary>
public partial class NewsModel : KonkordObservableObject
{
    /// <summary>
    /// The title of the news item.
    /// </summary>
    [ObservableProperty] private string title;

    /// <summary>
    /// The content of the news item.
    /// </summary>
    [ObservableProperty] private string content;

    /// <summary>
    /// The banner image associated with the news item, if available.
    /// </summary>
    [ObservableProperty] private Bitmap? banner;

    /// <summary>
    /// Initializes a new instance of the <see cref="NewsModel"/> class with the specified title, content, and banner image.
    /// </summary>
    /// <param name="title">The title of the news item.</param>
    /// <param name="content">The content of the news item.</param>
    /// <param name="banner">The banner image associated with the news item, or null if no banner is provided.</param>
    public NewsModel(string title, string content, Bitmap? banner)
    {
        Title = title;
        Content = content;
        Banner = banner;
    }
}
