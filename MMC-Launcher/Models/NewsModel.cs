using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.MesterMC.Launcher.Helpers;

namespace Tavstal.MesterMC.Launcher.Models;

public partial class NewsModel : KonkordObservableObject
{
    [ObservableProperty] private string title;
    [ObservableProperty] private string content;
    [ObservableProperty] private Bitmap bannerUrl;
    
    public NewsModel(string title, string content, string bannerUrl)
    {
        Title = title;
        Content = content;
        BannerUrl = ImageHelper.Load(bannerUrl).GetAwaiter().GetResult() ?? ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/post_image_01.jpg"));
    }
}