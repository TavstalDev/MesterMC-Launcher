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

public partial class UninstallViewModel : ObservableObject
{
    private readonly CoreLogger _logger = new(typeof(UninstallViewModel));
    
    [ObservableProperty] private EUninstallerWindow currentWindow;
    [ObservableProperty] private double installProgress;
    [ObservableProperty] private string installText = "";
    [ObservableProperty] private string reviewText = "...";
    
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);
    
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
                await Dispatcher.UIThread.InvokeAsync(async () => await StartAsync());
                return;
            }
        }
        
        CurrentWindow = window;
    }

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
                InstallText = "Hiba: Nem található a MesterMC telepítési könyvtára.";
                _logger.Error("The game directory path is empty.");
                return;
            }
            
            if (!await FileSystemHelper.HasWritePermissionAsync(gameDir))
            {
                InstallText = "Hiba: Nincs írási jogosultság a játék könyvtárához.";
                _logger.Error("No write permission for the game directory.");
                return;
            }

            if (!await FileSystemHelper.HasWritePermissionAsync(startMenuRootDir))
            {
                InstallText = "Hiba: Nincs írási jogosultság a Start menü könyvtárához.";
                _logger.Error("No write permission for the Start Menu directory.");
                return;
            }

            string launcherPath = Path.Combine(gameDir, "bin", InstallHelper.GetLauncherExecutableName());
            string updaterPath = Path.Combine(gameDir, "bin", InstallHelper.GetUpdaterExecutableName());
            if (FileSystemHelper.IsFileLocked(launcherPath) || FileSystemHelper.IsFileLocked(updaterPath))
            {
                InstallText = "Hiba: A MesterMC Launcher vagy Updater jelenleg fut. Kérlek zárd be őket a törlés előtt.";
                _logger.Error("The launcher or updater executable is currently running.");
                return;
            }
            
            InstallText = "Asztali ikon törlése...";
            InstallProgress = 1f / 3f;
            if (File.Exists(desktopShortcutPath))
                File.Delete(desktopShortcutPath);

            InstallText = "Startmenü ikon törlése...";
            InstallProgress = 2f / 3f;
            if (startMenuRootDir == startMenuShortcutDir)
            {
                if (File.Exists(startMenuShortcutPath))
                    File.Delete(startMenuShortcutPath);
            }
            else
            {
                if (Directory.Exists(startMenuShortcutDir) && Directory.GetFiles(startMenuShortcutDir).Length == 1)
                    Directory.Delete(startMenuShortcutDir, true);
            }
            
            InstallText = "Játék könyvtár törlése...";
            InstallProgress = 1.0f;
            if (Directory.Exists(gameDir))
                Directory.Delete(gameDir, true);
            
            InstallHelper.RemoveInstallPath();
            CurrentWindow = EUninstallerWindow.FINISH;
        }
        catch (Exception ex)
        {
            InstallText = $"Hiba történt a törlés során: {ex.Message}";
        }
    }
}