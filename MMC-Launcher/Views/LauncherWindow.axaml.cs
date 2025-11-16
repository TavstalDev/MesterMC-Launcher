using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.MesterMC.Launcher.Models;
using Tavstal.MesterMC.Launcher.Views.Models;

namespace Tavstal.MesterMC.Launcher.Views;

public partial class LauncherWindow : KonkordWindow<LauncherViewModel>
{
    private CoreLogger _logger = CoreLogger.WithModuleType(typeof(LauncherWindow));
    
    [RequiresUnreferencedCode("This constructor uses code that may be removed during trimming.")]
    public LauncherWindow()
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        
        DataContext ??= new LauncherViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.HideWindowInteraction.RegisterHandler(action =>
            {
                Hide();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
        
        _ = InitializeNewsAsync();
    }
    
    private async Task InitializeNewsAsync()
    {
        if (DataContext == null)
            return;
        
        try
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            var items = await LauncherHelper.GetNewsAsync(settings.Launcher.CacheDirectoryPath);

            if (items.Count == 0)
            {
                AddFallbackNews();
                return;
            }

            foreach (var item in items)
            {
                Bitmap image;

                try
                {
                    image = await ImageHelper.LoadFromWeb(item.GetBannerUri())
                            ?? ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/post_image_01.jpg"));
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to load news image.\n" + ex);
                    image = ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/post_image_01.jpg"));
                }

                DataContext.NewsItems.Add(new NewsModel(item.Title, item.Content, image));
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to load news items.\n" + ex);
            AddFallbackNews();
        }

        if (DataContext.NewsItems.Count > 0)
        {
            DataContext.SelectedNewsItem = DataContext.NewsItems[0];
            DataContext.SelectedNewsIndex = 0;
        }
    }
    
    private void AddFallbackNews()
    {
        if (DataContext == null)
            return;
        
        DataContext.NewsItems.Clear();
        DataContext.NewsItems.Add(new NewsModel(
            "Hiba",
            "Váratlan hiba történt a hírek betöltése során.",
            ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/post_image_01.jpg"))
        ));
    }


    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        FitToDisplay();
    }

    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            BeginMoveDrag(e);
        }
    }
    
    private void FitToDisplay()
    {
        double screenWidth = (double)App.ScreenWidth;
        double screenHeight = (double)App.ScreenHeight;
        
        // Base design resolution
        double baseWidth = 1280;
        double baseHeight = 665;

        // Calculate scale factor relative to screen
        double scaleX = screenWidth / baseWidth;
        double scaleY = screenHeight / baseHeight;

        // Pick whichever fits better
        /*double multiplier;
        if (screenHeight >= 1080 && screenWidth >= 1920)
            multiplier = 0.6;
        else
            multiplier = 0.47;*/
        
        double scale = Math.Min(scaleX, scaleY) * 0.55;

        // Apply scaled window size
        Width = baseWidth * scale;
        Height = baseHeight * scale;

        // Center window on screen
        Position = new PixelPoint(
            (int)((screenWidth - Width) / 2),
            (int)((screenHeight - Height) / 2)
        );
    }
}