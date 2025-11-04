using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Installer.Models;

namespace Tavstal.MesterMC.Installer.Views;

public partial class MainViewModel : ObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainViewModel));
    [ObservableProperty] private EInstallerWindow currentWindow;
    [ObservableProperty] private bool isLicenseAccepted;
    [ObservableProperty] private string gameDirectory;
    [ObservableProperty] private string startMenuDirectory;
    [ObservableProperty] private bool createDesktopShortcut = true;
    [ObservableProperty] private bool createStartMenuShortcut = false;
    public Interaction<Unit, Unit> CloseInteraction { get; } = new();
    
    [RelayCommand]
    private Task OpenWindow(EInstallerWindow window)
    {
        CurrentWindow = window;
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseInteraction.Handle(Unit.Default);
    }
    
    [RelayCommand]
    private async Task SelectGameDirectory()
    {
        
    }
    
    [RelayCommand]
    private async Task SelectStartMenuDirectory()
    {
        
    }
}