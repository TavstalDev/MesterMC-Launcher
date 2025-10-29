using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.MesterMC.Launcher.Models;

namespace Tavstal.MesterMC.Launcher.Views.Models;

public partial class LauncherViewModel : ObservableObject
{
    [ObservableProperty] private Bitmap logoImage;
    private ObservableCollection<NewsModel> newsItems = new ();

    public ObservableCollection<NewsModel> NewsItems
    {
        get => newsItems;
        set => SetProperty(ref newsItems, value);
    }
    
    [ObservableProperty] private string username;
    [ObservableProperty] private string password;
    [ObservableProperty] private bool offlineMode = true; // TODO: Set to false when online login is implemented
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(isLoggingIn))] private ELoginStatus loginStatus;
    public bool isLoggingIn => LoginStatus != ELoginStatus.NONE;
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    
    
    [RelayCommand]
    public async Task Login()
    {
        if (OfflineMode)
        {
            await PlayOffline();
            return;
        }
        
        // TODO: Implement online login logic here
    }
    
    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
    
    private async Task PlayOffline()
    {
        var instance = App.getInstance();
        if (instance == null)
            return;
        
        if (string.IsNullOrEmpty(Username))
            return;
        
        LoginStatus = ELoginStatus.SUCCESS;
        instance.UpdateUserDetails(new ClientDetails("0", Username, GameHelper.GetOfflinePlayerUUID(Username), true));
        await Task.Delay(250); // Small delay to ensure status update is visible
        LoginStatus = ELoginStatus.LAUNCHING;
        var process = await instance.Start();
        await Task.Delay(5000); // Wait for a bit to ensure the process has started
        if (process is { HasExited: false })
            await CloseWindowInteraction.Handle(Unit.Default);

    }
}