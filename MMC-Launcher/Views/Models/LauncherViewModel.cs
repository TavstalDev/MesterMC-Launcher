using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models.Json;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.MesterMC.Launcher.Models;

namespace Tavstal.MesterMC.Launcher.Views.Models;

[RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
public partial class LauncherViewModel : ObservableObject
{
    private CoreLogger _logger = CoreLogger.WithModuleType(typeof(LauncherViewModel));
    
    public ObservableCollection<NewsModel> NewsItems = [];
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NewsPageDisplay))] private int selectedNewsIndex;
    [ObservableProperty] private NewsModel? selectedNewsItem;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? username;
    [ObservableProperty] private string? password;
    [ObservableProperty] private string? tfaCode;
    [ObservableProperty] private string? tfaToken;
    [ObservableProperty] private bool offlineMode = true;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(isLoggingIn))] [NotifyPropertyChangedFor(nameof(isError))] [NotifyPropertyChangedFor(nameof(isTFA))] [NotifyPropertyChangedFor(nameof(shouldShowFeedback))] private ELoginStatus loginStatus;
    public ObservableCollection<string> SavedUsernames { get; set; } = new();
    public bool isLoggingIn => LoginStatus != ELoginStatus.NONE;
    public bool shouldShowFeedback => LoginStatus != ELoginStatus.NONE && LoginStatus != ELoginStatus.TFA;
    public bool isTFA => LoginStatus == ELoginStatus.TFA;
    public bool isError => LoginStatus == ELoginStatus.ERROR;
    public string NewsPageDisplay => $"{SelectedNewsIndex + 1} / {NewsItems.Count}";
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> HideWindowInteraction { get; } = new();

    public LauncherViewModel()
    {
        NewsItems.CollectionChanged += (_, _) =>
        {
            // 3. Manually notify that the property needs recalculation
            OnPropertyChanged(nameof(NewsPageDisplay));
        };

        var settings = LauncherHelper.GetLauncherSettings();
        foreach (var user in settings.Users.Keys)
            SavedUsernames.Add(user);
    }

    #region Relay Commands
    [RelayCommand]
    public async Task Login()
    {
        string? playerName = Username;
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
        
        if (OfflineMode)
        {
            await PlayAsync("0", playerName);
            return;
        }

        if (string.IsNullOrEmpty(Password))
        {
            LoginStatus = ELoginStatus.ERROR;
            ErrorMessage = "A bejelentkezéshez szükséges a jelszó megadása.";
            return;
        }

        LoginStatus = ELoginStatus.LOGGING_IN;
        var result = await AuthHelper.LoginAsync(playerName, Password);
        if (result == null)
        {
            LoginStatus = ELoginStatus.ERROR;
            ErrorMessage = "Hiba történt a bejelentkezés során. Kérlek, ellenőrizd a felhasználóneved és jelszavad, majd próbáld újra.";
            return;
        }

        // 2FA required
        if (result.Value.Item2)
        {
            TfaToken = result.Value.Item1;
            LoginStatus = ELoginStatus.TFA;
            return;
        }
        
        // Successful login
        await PlayAsync(result.Value.Item1, playerName);
    }
    
    [RelayCommand]
    public async Task SubmitTFA()
    {
        if (string.IsNullOrEmpty(TfaCode) && TfaCode!.Length != 6)
            return;

        LoginStatus = ELoginStatus.LOGGING_IN;
        var result = await AuthHelper.SubmitTFA(TfaToken!, TfaCode); // TfaToken is guaranteed to be non-null here
        if (string.IsNullOrEmpty(result))
        {
            LoginStatus = ELoginStatus.ERROR;
            ErrorMessage = "Hiba történt a kétlépcsős azonosítás során. Kérlek, ellenőrizd a kódot, majd próbáld újra.";
            return;
        }

        // Successful TFA
        await PlayAsync(result, Username!); // Username is guaranteed to be non-null here
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

    [RelayCommand]
    public async Task RemoveUser(string user)
    {
        if (!SavedUsernames.Contains(user))
            return;

        SavedUsernames.Remove(user);
        var config = await LauncherHelper.GetLauncherSettingsAsync();
        if (config.Users.Remove(user))
        {
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, config, CommonJsonContext.Default.CoreConfig);
        }
    }
    #endregion
    
    private async Task PlayAsync(string accessToken, string playerName)
    {
        var instance = App.getInstance();
        if (instance == null)
            return;
        
        LoginStatus = ELoginStatus.SUCCESS;
        var config = await LauncherHelper.GetLauncherSettingsAsync();
        config.Users.TryAdd(playerName, accessToken);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, config, CommonJsonContext.Default.CoreConfig);
        
        instance.UpdateUserDetails(new ClientDetails(accessToken, playerName, GameHelper.GetOfflinePlayerUUID(playerName), true));
        await Task.Delay(250); // Small delay to ensure status update is visible
        LoginStatus = ELoginStatus.LAUNCHING;
        //await Task.Run(async () => await MetricsHelper.SendMetricAsync()); // Send metrics in the background, tracks basic hardware info (cpu, ram, gpu, sum of disk space), so we can track what to optimize for and how our userbase looks like
        var process = await instance.Start();
        App.ClearRPC();
        await Task.Delay(5000); // Wait for a bit to ensure the process has started
        if (process is { HasExited: false })
        {
            await HideWindowInteraction.Handle(Unit.Default);
            process.Exited += async (_, _) => await CloseWindowInteraction.Handle(Unit.Default); 
        }
        else
        {
            ErrorMessage = "Váratlan hiba történt a játék elindítása során.";
            LoginStatus = ELoginStatus.ERROR;
        }
    }
}