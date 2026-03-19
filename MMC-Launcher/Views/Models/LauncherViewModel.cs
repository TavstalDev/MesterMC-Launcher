using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.MesterMC.Launcher.Helpers;
using Tavstal.MesterMC.Launcher.Models;
using Tavstal.MesterMC.Launcher.Models.Config.DTOs;
using Tavstal.MesterMC.Launcher.Models.Json;
using CoreConfigModel = Tavstal.MesterMC.Launcher.Models.Config.CoreConfigModel;

namespace Tavstal.MesterMC.Launcher.Views.Models;

/// <summary>
/// View model for the launcher main window.
/// Handles user login, news management, configuration persistence and starting the game process.
/// </summary>
[RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
public partial class LauncherViewModel : ObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(LauncherViewModel));
    private readonly bool _isInitialized;
    private Process? gameProcess;
    private DateTime startTime = DateTime.UtcNow;
    public bool IsLinux => OSHelper.GetOperatingSystem() == EOperatingSystem.Linux;
    
    #region Observable Properties
    [ObservableProperty] private ESettingsCategory _currentSettingsCategory = ESettingsCategory.WINDOW;
    [ObservableProperty] private CoreConfigModel _coreConfig;
    public ObservableCollection<NewsModel> NewsItems = [];
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(NewsPageDisplay))] private int selectedNewsIndex;
    [ObservableProperty] private NewsModel? selectedNewsItem;
    [ObservableProperty] private string? errorMessage;
    [ObservableProperty] private string? username;
    [ObservableProperty] private string? password;
    [ObservableProperty] private string? tfaCode;
    [ObservableProperty] private string? tfaToken;
    [ObservableProperty] private bool offlineMode;
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

    /// <summary>
    /// Initializes a new instance of <see cref="LauncherViewModel"/>.
    /// </summary>
    public LauncherViewModel()
    {
        _coreConfig = new CoreConfigModel(LauncherHelper.GetLauncherSettings());
        NewsItems.CollectionChanged += (_, _) =>
        {
            // 3. Manually notify that the property needs recalculation
            OnPropertyChanged(nameof(NewsPageDisplay));
        };

        var settings = LauncherHelper.GetLauncherSettings();
        if (settings.Users.Count > 0)
        {
            foreach (var user in settings.Users.Keys)
                SavedUsernames.Add(user);
            OfflineMode = string.IsNullOrEmpty(settings.Users.Values.ElementAt(0));
        }

        _isInitialized = true;
        SubscribeToCoreConfigChildren(_coreConfig);
    }

    #region Relay Commands
    /// <summary>
    /// Attempt to log in with the provided Username and Password.
    /// Performs client-side validation, handles offline mode, and triggers PlayAsync on success.
    /// Sets <see cref="LoginStatus"/> and <see cref="ErrorMessage"/> appropriately.
    /// </summary>
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
        await PlayAsync(result.Value.Item1, playerName, result.Value.Item3);
    }
    
    /// <summary>
    /// Submit a two-factor authentication code.
    /// On success continues to start the game via PlayAsync.
    /// </summary>
    [RelayCommand]
    public async Task SubmitTFA()
    {
        if (string.IsNullOrEmpty(TfaCode) && TfaCode!.Length != 6)
            return;

        LoginStatus = ELoginStatus.LOGGING_IN;
        var result = await AuthHelper.SubmitTFA(TfaToken!, TfaCode); // TfaToken is guaranteed to be non-null here
        if (result == null)
        {
            LoginStatus = ELoginStatus.ERROR;
            ErrorMessage = "Hiba történt a kétlépcsős azonosítás során. Kérlek, ellenőrizd a kódot, majd próbáld újra.";
            return;
        }
        string accessToken = result.Value.Item1;
        string uuid = result.Value.Item2;

        // Successful TFA
        await PlayAsync(accessToken, Username!, uuid); // Username is guaranteed to be non-null here
    }

    /// <summary>
    /// Navigate back from TFA or settings UI state to the login state.
    /// </summary>
    [RelayCommand]
    public async Task Back()
    {
        SettingsOpened = false;
        LoginStatus = !string.IsNullOrEmpty(TfaCode) && LoginStatus == ELoginStatus.ERROR ? ELoginStatus.TFA : ELoginStatus.NONE;
        ErrorMessage = string.Empty;
        await Task.CompletedTask;
    }
    
    /// <summary>
    /// Request the view to close the window via interaction.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
    
    /// <summary>
    /// Select the previous news item (wraps to the last).
    /// Updates SelectedNewsItem after changing SelectedNewsIndex.
    /// </summary>
    [RelayCommand]
    public async Task PreviousNews()
    {
        if (SelectedNewsIndex > 0)
            SelectedNewsIndex--;
        else
            SelectedNewsIndex = NewsItems.Count - 1;

        SelectedNewsItem = NewsItems[SelectedNewsIndex];
        await Task.CompletedTask;
    }

    /// <summary>
    /// Select the next news item (wraps to the first).
    /// Updates SelectedNewsItem after changing SelectedNewsIndex.
    /// </summary>
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

    /// <summary>
    /// Remove a saved username from the local list and persist the change to disk.
    /// </summary>
    [RelayCommand]
    public async Task RemoveUser(string user)
    {
        if (!SavedUsernames.Contains(user))
            return;

        SavedUsernames.Remove(user);
        var config = await LauncherHelper.GetLauncherSettingsAsync();
        if (config.Users.Remove(user))
        {
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, config, CustomJsonContext.Default.CoreConfigDto);
        }
    }

    /// <summary>
    /// Open the settings UI.
    /// </summary>
    [RelayCommand]
    public async Task OpenSettings()
    {
        if (SettingsOpened)
            return;
        
        SettingsOpened = true;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Switch the visible settings category in the UI.
    /// </summary>
    [RelayCommand]
    public async Task HandleSettingsCategory(ESettingsCategory category)
    {
        if (category == CurrentSettingsCategory)
            return;
        
        CurrentSettingsCategory = category;
        await Task.CompletedTask;
    }
    #endregion
    
    /// <summary>
    /// Creates a Minecraft instance, updates user data and starts the process.
    /// Persists the last used user and saved users list.
    /// Attaches process event handlers to monitor startup and exit.
    /// </summary>
    /// <param name="accessToken">Authentication access token (or "0" for offline).</param>
    /// <param name="playerName">Player name to use.</param>
    /// <param name="uuid">Optional UUID; if null an offline UUID is generated.</param>
    private async Task PlayAsync(string accessToken, string playerName, string? uuid = null)
    {
        MinecraftInstance? instance = App.createMinecraftInstance(null);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (instance == null) 
            return;
        
        LoginStatus = ELoginStatus.SUCCESS;
        bool isOffline = accessToken == "0";
        var config = await LauncherHelper.GetLauncherSettingsAsync();
        config.Users.TryAdd(playerName, isOffline ? "" : "reserved for future use"); // Maybe it will be used in the future.
        config.LastUser = config.Users.Keys.ToList().IndexOf(playerName);
        await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, config, CustomJsonContext.Default.CoreConfigDto);
        
        uuid ??= GameHelper.GetOfflinePlayerUUID(playerName);
        instance.UpdateUserDetails(new ClientDetails(accessToken, playerName, uuid, true));
        await Task.Delay(250); // Small delay to ensure status update is visible
        LoginStatus = ELoginStatus.LAUNCHING;
        
        startTime = DateTime.UtcNow;
        gameProcess = await instance.Start();
        if (gameProcess != null)
        {
            App.ClearRPC();
            gameProcess.OutputDataReceived += HandleGameOutput;
            gameProcess.Exited += async (_, _) => await Dispatcher.UIThread.InvokeAsync(async () =>  await CloseWindowInteraction.Handle(Unit.Default));
        }
        else
        {
            ErrorMessage = "Váratlan hiba történt a játék elindítása során.";
            LoginStatus = ELoginStatus.ERROR;
        }
    }
    
    /// <summary>
    /// Handle output from the game process to detect when Minecraft has started.
    /// When detected, hides the launcher window and unsubscribes from further output.
    /// </summary>
    private void HandleGameOutput(object? sender, DataReceivedEventArgs e)
    {
        if (gameProcess == null || e.Data == null)
            return;

        if (e.Data.Contains("Render thread") ||
            e.Data.Contains("LWJGL Version") ||
            e.Data.Contains("Backend library") ||
            e.Data.Contains("Starting Minecraft"))
        {
            Dispatcher.UIThread.Invoke(async () =>
            {
                DateTime endTime = DateTime.UtcNow;
                _logger.Info("Minecraft launched in " + (endTime - startTime).TotalMilliseconds + "ms.");
                await HideWindowInteraction.Handle(Unit.Default);
                gameProcess.OutputDataReceived -= HandleGameOutput;
            });
        }
    }
    
    #region Config Management
    /// <summary>
    /// Subscribe to PropertyChanged events of core config child models (Java/Window/Performance).
    /// Used to persist changes.
    /// </summary>
    private void SubscribeToCoreConfigChildren(CoreConfigModel config)
    {
        config.Java.PropertyChanged += OnChildConfigPropertyChanged;
        config.Window.PropertyChanged += OnChildConfigPropertyChanged;
        config.Performance.PropertyChanged += OnChildConfigPropertyChanged;
    }
    
    /// <summary>
    /// Unsubscribe from previously subscribed child property change events.
    /// </summary>
    private void UnsubscribeFromCoreConfigChildren(CoreConfigModel config)
    {
        config.Java.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Window.PropertyChanged -= OnChildConfigPropertyChanged;
        config.Performance.PropertyChanged -= OnChildConfigPropertyChanged;
    }
    
    /// <summary>
    /// Called when the CoreConfig property is replaced.
    /// Re-subscribes to child change notifications and, if initialized, saves to disk.
    /// </summary>
    partial void OnCoreConfigChanged(CoreConfigModel? oldValue, CoreConfigModel newValue)
    {
        if (oldValue != null)
            UnsubscribeFromCoreConfigChildren(oldValue);

        SubscribeToCoreConfigChildren(newValue);

        if (!_isInitialized)
            return;
        SaveCoreConfigToFile(newValue);
    }
    
    /// <summary>
    /// Handler for child property changes that triggers persistence when initialized.
    /// </summary>
    private void OnChildConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!_isInitialized)
            return;

        SaveCoreConfigToFile(CoreConfig);
    }
    
    /// <summary>
    /// Serialize the relevant parts of the core config and write them to the launcher config file.
    /// Preserves launcher-level non-observable properties by reading the existing settings first.
    /// </summary>
    private void SaveCoreConfigToFile(CoreConfigModel newValue)
    {
        var oldSettings = LauncherHelper.GetLauncherSettings(); // Fetch to preserve non-observable properties

        if (newValue.Java.MinMemory > newValue.Java.MaxMemory)
            newValue.Java.MinMemory = newValue.Java.MaxMemory;

        var settings = new CoreConfigDto
        {
            Launcher = oldSettings.Launcher, // Preserve non-observable properties
            Java = new JavaConfigDto
            {
                MinMemory = newValue.Java.MinMemory,
                MaxMemory = newValue.Java.MaxMemory,
                PermaGen = newValue.Java.PermaGen
            },
            Minecraft = new MinecraftConfigDto
            {
                StartMaximized = newValue.Window.StartMaximized,
                WindowHeight = newValue.Window.WindowHeight,
                WindowWidth = newValue.Window.WindowWidth,
            },
            Misc = new MiscConfigDto
            {
                UseCustomGlfw = newValue.Performance.UseCustomGlfw,
                CustomGlfwPath = newValue.Performance.CustomGlfwPath!,
                UseCustomOpenAl = newValue.Performance.UseCustomOpenAl,
                CustomOpenAlPath = newValue.Performance.CustomOpenAlPath!,
                UseDedicatedGpu = newValue.Performance.UseDedicatedGpu,
                EnableMangoHud = newValue.Performance.EnableMangoHud,
                EnableFeralGameMode = newValue.Performance.EnableFeralGameMode,
            },
            CacheRefreshDate = oldSettings.CacheRefreshDate
        };

        JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, settings, CustomJsonContext.Default.CoreConfigDto);
    }

    #endregion
}