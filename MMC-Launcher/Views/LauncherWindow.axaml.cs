using System;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using ReactiveUI;
using Tavstal.MesterMC.Launcher.Helpers;
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