using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Tavstal.MesterMC.Updater.Views;

namespace Tavstal.MesterMC.Updater;

// ReSharper disable once PartialTypeWithSinglePart
[RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
public partial class App : Application
{
    #region Screen Size
    private static PixelSize _screenSize = new(1920, 1080);
    public static PixelSize ScreenSize => _screenSize;
    
    public static decimal ScreenWidth => _screenSize.Width;
    public static decimal ScreenHeight => _screenSize.Height;
    public static void SetScreenSize(PixelSize screenSize)
    {
        _screenSize = screenSize;
    }
    #endregion
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new UpdateWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}