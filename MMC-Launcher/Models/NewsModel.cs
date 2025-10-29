using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;

namespace Tavstal.MesterMC.Launcher.Models;

public partial class NewsModel : KonkordObservableObject
{
    [ObservableProperty] private string title;
    [ObservableProperty] private string content;
    [ObservableProperty] private Bitmap? banner;
    
    public NewsModel(string title, string content, string bannerUrl)
    {
        Title = title;
        Content = content;
        Banner = ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/post_image_01.jpg"));
    }
}