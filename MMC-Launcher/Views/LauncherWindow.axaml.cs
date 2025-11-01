using System;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Launcher.Models;
using Tavstal.MesterMC.Launcher.Views.Models;

namespace Tavstal.MesterMC.Launcher.Views;

public partial class LauncherWindow : KonkordWindow<LauncherViewModel>
{
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
        });
        
        // Set window size
        //this.Width = (double)App.ScreenWidth * 0.7;
        //this.Height = (double)App.ScreenHeight * 0.45;
        
        // Add initial news
        DataContext.NewsItems.Add(new NewsModel("Friss hírek betöltése...", "Kérlek várj, amíg a hírek betöltődnek.", "avares://MMC-Launcher/Assets/news/banners/loading-news.png"));
        DataContext.NewsItems.Add(new NewsModel("Hiba a hírek betöltése közben", "Sajnáljuk, de nem sikerült betölteni a híreket. Kérlek ellenőrizd az internetkapcsolatodat, vagy próbáld újra később.", "avares://MMC-Launcher/Assets/news/banners/loading-news.png"));
        DataContext.NewsItems.Add(new NewsModel("Üdvözlünk a MesterMC Launcherben!", "Köszönjük, hogy a MesterMC Launchert választottad! Itt megtalálod a legfrissebb híreket, frissítéseket és eseményeket a MesterMC közösségből.", "avares://MMC-Launcher/Assets/news/banners/loading-news.png"));
        
        DataContext.SelectedNewsItem = DataContext.NewsItems[0];
        DataContext.SelectedNewsIndex = 0;
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
        
        double scale = Math.Min(scaleX, scaleY) * 0.47;

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