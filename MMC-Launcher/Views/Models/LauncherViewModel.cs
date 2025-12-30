using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.Json;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.MesterMC.Launcher.Models;
using Tavstal.MesterMC.Launcher.Models.Config;

namespace Tavstal.MesterMC.Launcher.Views.Models;

[RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
public partial class LauncherViewModel : ObservableObject
{
    private readonly bool _isInitialized;
    public bool IsLinux => OSHelper.GetOperatingSystem() == EOperatingSystem.Linux;
    
    #region Observable Properties
    [ObservableProperty] private ESettingsCategory _currentSettingsCategory = ESettingsCategory.Window;
    [ObservableProperty] private CoreConfigModel _coreConfig;
    public ObservableCollection<NewsModel> NewsItems = [];
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NewsPageDisplay))] private int selectedNewsIndex;
    [ObservableProperty] private NewsModel? selectedNewsItem;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? username;
    [ObservableProperty] private string? password;
    [ObservableProperty] private string? tfaCode;
    [ObservableProperty] private string? tfaToken;
    [ObservableProperty] private bool offlineMode = true;
    [ObservableProperty] private bool settingsOpened;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(isLoggingIn))] [NotifyPropertyChangedFor(nameof(isError))] [NotifyPropertyChangedFor(nameof(isTFA))] [NotifyPropertyChangedFor(nameof(shouldShowFeedback))] private ELoginStatus loginStatus;
    public ObservableCollection<string> SavedUsernames { get; set; } = new();
    #endregion
    public bool isLoggingIn => LoginStatus != ELoginStatus.NONE;
    public bool shouldShowFeedback => LoginStatus != ELoginStatus.NONE && LoginStatus != ELoginStatus.TFA;
    public bool isTFA => LoginStatus == ELoginStatus.TFA;
    public bool isError => LoginStatus == ELoginStatus.ERROR;
    public string NewsPageDisplay => $"{SelectedNewsIndex + 1} / {NewsItems.Count}";
    #region Interactions
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    public Interaction<Unit, Unit> HideWindowInteraction { get; } = new();
    public Interaction<Unit, string?> OpenFolderPicker { get; } = new();
    #endregion

    public LauncherViewModel()
    {
        _coreConfig = new CoreConfigModel(LauncherHelper.GetLauncherSettings());
        NewsItems.CollectionChanged += (_, _) =>
        {
            // 3. Manually notify that the property needs recalculation
            OnPropertyChanged(nameof(NewsPageDisplay));
        };

        var settings = LauncherHelper.GetLauncherSettings();
        foreach (var user in settings.Users.Keys)
            SavedUsernames.Add(user);
        
        _isInitialized = true;
        SubscribeToCoreConfigChildren(_coreConfig);
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
            TfaCode = string.Empty;
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
        SettingsOpened = false;
        LoginStatus = !string.IsNullOrEmpty(TfaCode) && LoginStatus == ELoginStatus.ERROR ? ELoginStatus.TFA : ELoginStatus.NONE;
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

    [RelayCommand]
    public async Task OpenSettings()
    {
        if (SettingsOpened)
            return;
        
        SettingsOpened = true;
        await Task.CompletedTask;
    }

    [RelayCommand]
    public async Task HandleSettingsCategory(ESettingsCategory category)
    {
        if (category == CurrentSettingsCategory)
            return;
        
        CurrentSettingsCategory = category;
        await Task.CompletedTask;
    }
    
    [RelayCommand]
    public async Task ConfigDirSelectAsync()
    {
        var directoryResult = await OpenFolderPicker.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;
        
        CoreConfig.Java.DefaultJavaPath = directoryResult;
    }
    #endregion
    
    private async Task PlayAsync(string accessToken, string playerName)
    {
        MinecraftInstance? instance = App.createMinecraftInstance(null);
        if (instance == null) 
            return;
        
        LoginStatus = ELoginStatus.SUCCESS;
        var config = await LauncherHelper.GetLauncherSettingsAsync();
        config.Users.TryAdd(playerName, accessToken);
        config.LastUser = config.Users.Keys.ToList().IndexOf(playerName);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, config, CommonJsonContext.Default.CoreConfig);
        
        instance.UpdateUserDetails(new ClientDetails(accessToken, playerName, GameHelper.GetOfflinePlayerUUID(playerName), true));
        await Task.Delay(250); // Small delay to ensure status update is visible
        LoginStatus = ELoginStatus.LAUNCHING;
        //await Task.Run(async () => await MetricsHelper.SendMetricAsync()); // Send metrics in the background, tracks basic hardware info (cpu, ram, gpu, sum of disk space), so we can track what to optimize for and how our userbase looks like
        var process = await instance.Start();
        if (process != null)
        {
            App.ClearRPC();
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null)
                    return;

                if (e.Data.Contains("Render thread") ||
                    e.Data.Contains("LWJGL Version") ||
                    e.Data.Contains("Backend library") ||
                    e.Data.Contains("Starting Minecraft"))
                {
                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await HideWindowInteraction.Handle(Unit.Default);
                    });
                }
            };
            process.Exited += async (_, _) => await Dispatcher.UIThread.InvokeAsync(async () =>  await CloseWindowInteraction.Handle(Unit.Default));
        }
        else
        {
            ErrorMessage = "Váratlan hiba történt a játék elindítása során.";
            LoginStatus = ELoginStatus.ERROR;
        }
    }
    
    #region Config Management
    
    private void SubscribeToCoreConfigChildren(CoreConfigModel config)
    {
        config.Java.PropertyChanged += OnChildConfigPropertyChanged;
        config.Window.PropertyChanged += OnChildConfigPropertyChanged;
        config.Performance.PropertyChanged += OnChildConfigPropertyChanged;
    }
    
    private void UnsubscribeFromCoreConfigChildren(CoreConfigModel config)
    {
        config.Java.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Window.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Performance.PropertyChanged -= OnChildConfigPropertyChanged;
    }
    
    partial void OnCoreConfigChanged(CoreConfigModel? oldValue, CoreConfigModel newValue)
    {
        if (oldValue != null)
            UnsubscribeFromCoreConfigChildren(oldValue);

        SubscribeToCoreConfigChildren(newValue);

        if (!_isInitialized)
            return;
        SaveCoreConfigToFile(newValue);
    }
    
    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isInitialized)
            return;

        SaveCoreConfigToFile(CoreConfig);
    }
    
    private void SaveCoreConfigToFile(CoreConfigModel newValue)
    {
        var oldSettings = LauncherHelper.GetLauncherSettings(); // Fetch to preserve non-observable properties

        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
            newValue.Java.MinMemory = newValue.Java.MaxMemory;

        var settings = new CoreConfig
        {
            Launcher = oldSettings.Launcher, // Preserve non-observable properties
            Java = new JavaConfig
            {
                MinMemory = newValue.Java.MinMemory,
                MaxMemory = newValue.Java.MaxMemory,
                PermaGen = newValue.Java.PermaGen,
                JavaPath = newValue.Java.DefaultJavaPath,
                JvmArguments = newValue.Java.JvmArguments,
            },
            Minecraft = new MinecraftConfig
            {
                StartMaximized = newValue.Window.StartMaximized,
                WindowHeight = newValue.Window.WindowHeight,
                WindowWidth = newValue.Window.WindowWidth,
            },
            Misc = new MiscConfig
            {
                PreLaunchCommand = newValue.Performance.PreLaunchCommand,
                WrapperCommand = newValue.Performance.WrapperCommand,
                PostExitCommand = newValue.Performance.PostExitCommand,
                UseCustomGlfw = newValue.Performance.UseCustomGlfw,
                CustomGlfwPath = newValue.Performance.CustomGlfwPath,
                UseCustomOpenAl = newValue.Performance.UseCustomOpenAl,
                CustomOpenAlPath = newValue.Performance.CustomOpenAlPath,
                UseDedicatedGpu = newValue.Performance.UseDedicatedGpu,
                EnableMangoHud = newValue.Performance.EnableMangoHud,
                EnableFeralGameMode = newValue.Performance.EnableFeralGameMode,
            },
            CacheRefreshDate = oldSettings.CacheRefreshDate
        };

        JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, settings, CommonJsonContext.Default.CoreConfig);
    }

    #endregion
}