using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Updater.Models;

namespace Tavstal.MesterMC.Updater.Views.Models;

/// <summary>
/// ViewModel used by the uninstaller UI.
/// Manages the uninstallation flow: prepares a review text, runs the uninstallation steps (delete shortcuts and installation folder),
/// reports progress/status to the UI and logs errors.
/// </summary>
public partial class UninstallViewModel : ObservableObject, IProgressReporter
{
    private readonly CoreLogger _logger = new(typeof(UninstallViewModel));
    
    [ObservableProperty] private EUninstallerWindow currentWindow;
    [ObservableProperty] private double installProgress;
    [ObservableProperty] private string installText = "";
    [ObservableProperty] private string reviewText = "...";
    
    /// <summary>
    /// Interaction used to request the window to close. The view should handle this to
    /// close the window in a UI-safe way.
    /// </summary>
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    
    /// <summary>
    /// Command that triggers closing the window via <see cref="CloseWindowInteraction"/>.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);
    
    /// <summary>
    /// Command that opens a given uninstaller window state.
    /// </summary>
    /// <param name="window">Target window state to open.</param>
    [RelayCommand]
    private async Task OpenWindow(EUninstallerWindow window)
    {
        switch (window)
        {
            case EUninstallerWindow.REVIEW:
            {
                string shortcutName = InstallHelper.GetShortcutName();
                string gameDir = InstallHelper.GetInstallPath() ?? "Nem található.";
                string startMenuPath = Path.Combine(InstallHelper.GetStartMenuPath() ?? OSHelper.GetProgramsDirectory(), shortcutName);
                string desktopShortcutPath = Path.Combine(OSHelper.GetDesktopDirectory(), shortcutName);
                
                StringBuilder reviewBuilder = new StringBuilder();
                reviewBuilder.AppendLine("Az alábbi elérési útvonalakon található fájlok törlésre kerülnek, kérlek nézd át figyelmesen.");
                reviewBuilder.AppendLine("A MesterMC Launcher főkönyvtára:");
                reviewBuilder.AppendLine($" {gameDir}");
                reviewBuilder.AppendLine("Az asztali indítóikon:");
                reviewBuilder.AppendLine($" {desktopShortcutPath}");
                reviewBuilder.AppendLine("A Start menübe helyezett indító:");
                reviewBuilder.AppendLine($" {startMenuPath}");
                reviewBuilder.AppendLine("");
                reviewBuilder.AppendLine("Megjegyzés: A felhasználói beállítások és mentések is törlődnek.");
                ReviewText = reviewBuilder.ToString();
                break;
            }
            case EUninstallerWindow.PROGRESS:
            {
                // Start installation process
                CurrentWindow = window;
                await Task.Run(async () => await StartAsync());
                return;
            }
        }
        
        CurrentWindow = window;
    }

    /// <summary>
    /// Executes the uninstallation procedure.
    /// <br/>Steps:
    /// <br/>1. Validate install path and write permissions.
    /// <br/>2. Check whether launcher/updater executables are running and abort if so.
    /// <br/>3. Delete desktop shortcut, start menu shortcut/folder, then installation directory.
    /// <br/>4. Remove stored install path via <see cref="InstallHelper.RemoveInstallPath"/>.
    /// <br/>5. Set final window state to FINISH on the UI thread.
    /// <br/>
    /// <br/>All UI-visible status and progress updates are performed via <see cref="SetStatus"/> and <see cref="SetProgress"/>,
    /// which marshal updates to the Avalonia UI thread. Exceptions are caught, logged and reported to the user through status text.
    /// </summary>
    private async Task StartAsync()
    {
        try
        {
            string shortcutName = InstallHelper.GetShortcutName();
            string? gameDir = InstallHelper.GetInstallPath();
            string startMenuRootDir = OSHelper.GetProgramsDirectory();
            string startMenuShortcutPath = Path.Combine(InstallHelper.GetStartMenuPath() ?? startMenuRootDir, shortcutName);
            string startMenuShortcutDir = Path.GetDirectoryName(startMenuShortcutPath) ?? startMenuRootDir;
            string desktopShortcutPath = Path.Combine(OSHelper.GetDesktopDirectory(), shortcutName);

            if (string.IsNullOrEmpty(gameDir))
            {
                SetStatus("Hiba: Nem található a MesterMC telepítési könyvtára.");
                _logger.Error("The game directory path is empty.");
                return;
            }
            
            if (!await FileSystemHelper.HasWritePermissionAsync(gameDir))
            {
                SetStatus("Hiba: Nincs írási jogosultság a játék könyvtárához.");
                _logger.Error("No write permission for the game directory.");
                return;
            }

            if (!await FileSystemHelper.HasWritePermissionAsync(startMenuRootDir))
            {
                SetStatus("Hiba: Nincs írási jogosultság a Start menü könyvtárához.");
                _logger.Error("No write permission for the Start Menu directory.");
                return;
            }

            string launcherPath = Path.Combine(gameDir, "bin", InstallHelper.GetLauncherExecutableName());
            string updaterPath = Path.Combine(gameDir, "bin", InstallHelper.GetUpdaterExecutableName());
            if (FileSystemHelper.IsFileLocked(launcherPath) || FileSystemHelper.IsFileLocked(updaterPath))
            {
                SetStatus("Hiba: A MesterMC Launcher vagy Updater jelenleg fut. Kérlek zárd be őket a törlés előtt.");
                _logger.Error("The launcher or updater executable is currently running.");
                return;
            }
            
            SetStatus("Asztali ikon törlése...");
            SetProgress(33.0);
            FileSystemHelper.DeleteFile(desktopShortcutPath, this);

            SetStatus("Startmenü ikon törlése...");
            SetProgress(66.0);
            if (startMenuRootDir == startMenuShortcutDir)
            {
                FileSystemHelper.DeleteFile(startMenuShortcutPath, this);
            }
            else
            {
                if (Directory.Exists(startMenuShortcutDir) && Directory.GetFiles(startMenuShortcutDir).Length == 1)
                    FileSystemHelper.DeleteDirectory(startMenuShortcutDir, this);
            }
            
            SetStatus("Játék könyvtár törlése...");
            SetProgress(100.0);
            if (!FileSystemHelper.DeleteDirectory(gameDir, this))
                return;
            
            InstallHelper.RemoveInstallPath();
            Dispatcher.UIThread.Invoke(() =>
            {
                CurrentWindow = EUninstallerWindow.FINISH;
            });
        }
        catch (Exception ex)
        {
            SetStatus($"Hiba történt a törlés során: {ex.Message}");
            _logger.Error($"An error occurred during uninstallation: {ex}");
        }
    }
    
    #region IProgressReporter Implementation

    /// <summary>
    /// Sets the progress value for the startup process.
    /// </summary>
    /// <param name="progress">The progress value, typically between 0.0 and 1.0.</param>
    public void SetProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            InstallProgress = progress;
        });
    }

    /// <summary>
    /// Sets the status message for the startup process.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void SetStatus(string status)
    {
        Dispatcher.UIThread.Post(() =>
        {
            InstallText = status;
        });
    }

    /// <summary>
    /// Sets the status message with optional arguments.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    /// <param name="args">Optional arguments for formatting the status message.</param>
    public void SetStatus(string status, params object[]? args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (args == null || args.Length == 0)
            {
                InstallText = status;
                return;
            }
            InstallText = string.Format(status, args);
        });
    }

    public void Show() { }
    public void Hide() { }
    #endregion 
}