using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
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
    public ObservableCollection<NewsModel> NewsItems = [];
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NewsPageDisplay))] private int selectedNewsIndex;
    [ObservableProperty] private NewsModel? selectedNewsItem;
    [ObservableProperty] private string? errorMessage;
    
    [ObservableProperty] private string? username;
    [ObservableProperty] private string? password;
    [ObservableProperty] private bool offlineMode = true; // TODO: Set to false when online login is implemented
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(isLoggingIn))] [NotifyPropertyChangedFor(nameof(isError))] private ELoginStatus loginStatus;
    public bool isLoggingIn => LoginStatus != ELoginStatus.NONE;
    public bool isError => LoginStatus == ELoginStatus.ERROR;
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
    
    //#region Relay Commands
    [RelayCommand]
    public async Task Login()
    {
        if (OfflineMode)
        {
            await PlayAsync("0", Username, true);
            return;
        }
        
        // TODO: Implement online login logic here
    }

    [RelayCommand]
    public async Task Back()
    {
        LoginStatus = ELoginStatus.NONE;
        ErrorMessage = string.Empty;
        await Task.CompletedTask;
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
    //#endregion
    
    private async Task PlayAsync(string accessToken, string? playerName, bool isOffline)
    {
        var instance = App.getInstance();
        if (instance == null)
            return;

        if (string.IsNullOrEmpty(playerName))
        {
            LoginStatus = ELoginStatus.ERROR;
            ErrorMessage = "Kérlek, adj meg egy felhasználónevet.";
            return;
        }

        // Note:
        // Yes technically it checks the name at least 2 times for the same thing, but this way it's easier to give specific error messages.
        
        if (playerName.Length < 3)
        {
            LoginStatus = ELoginStatus.ERROR;
            ErrorMessage = "A felhasználónévnek legalább 3 karakter hosszúnak kell lennie.";
            return;
        }
        
        if (playerName.Length > 16)
        {
            LoginStatus = ELoginStatus.ERROR;
            ErrorMessage = "A felhasználónév legfeljebb 16 karakter hosszú lehet.";
            return;
        }
        
        if (!Regex.IsMatch(playerName, "^[a-zA-Z0-9_]{3,16}$"))
        {
            LoginStatus = ELoginStatus.ERROR;
            ErrorMessage = "A felhasználónév csak betűket, számokat és aláhúzásokat tartalmazhat.";
            return;
        }
        
        LoginStatus = ELoginStatus.SUCCESS;
        instance.UpdateUserDetails(new ClientDetails(accessToken, playerName, GameHelper.GetOfflinePlayerUUID(playerName), isOffline));
        await Task.Delay(250); // Small delay to ensure status update is visible
        LoginStatus = ELoginStatus.LAUNCHING;
        var process = await instance.Start();
        await Task.Delay(5000); // Wait for a bit to ensure the process has started
        if (process is { HasExited: false })
            await CloseWindowInteraction.Handle(Unit.Default);
    }
}