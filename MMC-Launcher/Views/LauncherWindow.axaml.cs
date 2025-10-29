using System;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Interactivity;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models;
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
        
        // Add initial news
        DataContext.NewsItems.Add(new NewsModel("Friss hírek betöltése...", "Kérlek várj, amíg a hírek betöltődnek.", "avares://MMC-Launcher/Assets/news/banners/loading-news.png"));
        DataContext.NewsItems.Add(new NewsModel("Hiba a hírek betöltése közben", "Sajnáljuk, de nem sikerült betölteni a híreket. Kérlek ellenőrizd az internetkapcsolatodat, vagy próbáld újra később.", "avares://MMC-Launcher/Assets/news/banners/loading-news.png"));
        DataContext.NewsItems.Add(new NewsModel("Üdvözlünk a MesterMC Launcherben!", "Köszönjük, hogy a MesterMC Launchert választottad! Itt megtalálod a legfrissebb híreket, frissítéseket és eseményeket a MesterMC közösségből.", "avares://MMC-Launcher/Assets/news/banners/loading-news.png"));
        
        DataContext.SelectedNewsItem = DataContext.NewsItems[0];
        DataContext.SelectedNewsIndex = 0;
        
        // Load logo based on holiday
        // TODO: Find holiday logos
        DataContext.LogoImage = ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/logos/mmc-logo.png"));
        /*var date = DateTime.Now;
        switch (date.Month)
        {
            // Easter
            case 4:
            {
                
                break;
            }
            // Halloween
            case 10:
            {
                DataContext.LogoImage = ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/logos/mmc-logo-halloween.png"));
                break;
            }
            // Christmas
            case 12:
            {
                
                break;
            }
            default:
            {
                DataContext.LogoImage = ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/logos/mmc-logo.png"));
                break;
            }
        }*/
    }
}