using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
using Tavstal.MesterMC.Launcher.Models;
using Tavstal.MesterMC.Launcher.Views.Models;

namespace Tavstal.MesterMC.Launcher.Views;

[RequiresUnreferencedCode("This class uses code that may be removed during trimming.")]
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
        // Temporal news item shown while loading, otherwise the content area would be empty
        DataContext.NewsItems.Add(new NewsModel(
            "Béta Vezió",
            "Üdvözlünk a MesterMC Launcher Béta verziójában! Kérlek vedd figyelembe, hogy ez még egy fejlesztés alatt álló kiadás, így előfordulhatnak hibák és hiányzó funkciók. Köszönjük a türelmed és támogatásod!\n\nAz alábbi funkciók szándékosan ki vannak kapcsolva a béta tesztelés idejére:\n- Hírek betöltése az internetről\n- Automatikus frissítések\n- Bejelentkezés a fiókba, csak offline mód érhető el, de így is tudsz csatlakozni a szerverre.\n\nHa bármilyen problémába ütközöl, kérlek jelezd nekünk a Discord szerverünkön keresztül: https://discord.gg/mestermc\n\nFigyelem! Mivel a launcher még nem rendelkezik code signing tanúsítvánnyal, az antivírus szoftverek hamis pozitív eredményeket adhatnak. Kérlek győződj meg róla, hogy a letöltött fájl a hivatalos forrásból származik.",
            ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/post_image_beta.png"))
        ));
        /*DataContext.NewsItems.Add(new NewsModel(
            "Betöltés...",
            "Hírek betöltése folyamatban, kérlek várj...",
            ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/post_image_01.jpg"))
        ));*/
        DataContext.SelectedNewsItem = DataContext.NewsItems[0];
        DataContext.SelectedNewsIndex = 0;
        
        
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
        
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        if (DataContext == null)
            return;
        
        try
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            if (settings.Users.Count > 0)
                DataContext.Username = settings.Users.Keys.ElementAt(0);
            /* Disabled for BETA testing
             var items = await LauncherHelper.GetNewsAsync(settings.Launcher.CacheDirectoryPath);
            
            if (items.Count == 0)
            {
                AddFallbackNews();
                return;
            }

            bool oldNewsRemoved = false;
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

                if (!oldNewsRemoved)
                {
                    DataContext.NewsItems.Clear();
                    oldNewsRemoved = true;
                }
                DataContext.NewsItems.Add(new NewsModel(item.Title, item.Content, image));
            }*/
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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        App.ClearRPC();
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
        double scale = Math.Min(scaleX, scaleY) * 0.5;

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