using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.MesterMC.Launcher.Models;

public partial class NewsModel : KonkordObservableObject
{
    [ObservableProperty] private string title;
    [ObservableProperty] private string content;
    [ObservableProperty] private Bitmap? banner;
    
    public NewsModel(string title, string content, Bitmap? banner)
    {
        Title = title;
        Content = content;
        Banner = banner;
    }
}