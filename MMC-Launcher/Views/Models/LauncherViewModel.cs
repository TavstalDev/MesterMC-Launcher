using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.MesterMC.Launcher.Models;

namespace Tavstal.MesterMC.Launcher.Views.Models;

public partial class LauncherViewModel : ObservableObject
{
    [ObservableProperty] private Bitmap logoImage;
    public ObservableCollection<NewsModel> NewsItems = [];
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NewsPageDisplay))] private int selectedNewsIndex;
    [ObservableProperty] private NewsModel selectedNewsItem;
    
    [ObservableProperty] private string username;
    [ObservableProperty] private string password;
    [ObservableProperty] private bool offlineMode = true; // TODO: Set to false when online login is implemented
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(isLoggingIn))] private ELoginStatus loginStatus;
    public bool isLoggingIn => LoginStatus != ELoginStatus.NONE;
    public string NewsPageDisplay => $"{SelectedNewsIndex + 1} / {NewsItems.Count}";
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    public LauncherViewModel()
    {
        NewsItems.CollectionChanged += (s, e) => 
        {
            // 3. Manually notify that the property needs recalculation
            OnPropertyChanged(nameof(NewsPageDisplay));
        };
    }
    
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
    
    [RelayCommand]
    public async Task PreviousNews()
    {
        if (SelectedNewsIndex > 0)
        {
            SelectedNewsIndex--;
        }
        else
        {
            SelectedNewsIndex = NewsItems.Count - 1;
        }

        SelectedNewsItem = NewsItems[SelectedNewsIndex];
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task NextNews()
    {
        if (SelectedNewsIndex + 1 > NewsItems.Count - 1)
            SelectedNewsIndex = 0;
        else
            SelectedNewsIndex++;
        
        SelectedNewsItem = NewsItems[SelectedNewsIndex];
        await Task.CompletedTask;
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