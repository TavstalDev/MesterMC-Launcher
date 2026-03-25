using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ReactiveUI;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Installer.Models;
using ShellLink;

namespace Tavstal.MesterMC.Installer.Views;

/// <summary>
/// ViewModel for the installer main window.
/// </summary>
// TODO: Test error handling and the backup system
public partial class MainViewModel : ObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainViewModel));
    public readonly string HomeDirectory = OSHelper.GetHomeDirectory();
    public readonly string DesktopDirectory = OSHelper.GetDesktopDirectory();
    public readonly string StartmenuDirectory = OSHelper.GetProgramsDirectory();
    
    [ObservableProperty] private EInstallerWindow currentWindow;
    [ObservableProperty] private bool isLicenseAccepted;
    [ObservableProperty] private string? gameDirectory;
    [ObservableProperty] private string? startMenuDirectory;
    [ObservableProperty] private bool createDesktopShortcut = true;
    [ObservableProperty] private bool createStartMenuShortcut = true;
    [ObservableProperty] private bool openWebsite = true;
    [ObservableProperty] private bool launchGame = true;
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasPathErrorMessage))] private string? pathErrorMessage;
    [ObservableProperty] private string reviewText = string.Empty;
    [ObservableProperty] private double installProgress;
    [ObservableProperty] private string installText = string.Empty;
    public bool HasPathErrorMessage => !string.IsNullOrEmpty(PathErrorMessage);
    public Interaction<Unit, Unit> CloseInteraction { get; } = new();
    public Interaction<Unit, string?> DirPickerInteraction { get; } = new();
    
    #region Relay Commands
    /// <summary>
    /// Opens the specified installer window (wizard step) after validating inputs required by that step.
    /// </summary>
    [RelayCommand]
    private async Task OpenWindow(EInstallerWindow window)
    {
        switch (window)
        {
            case EInstallerWindow.LOCATION_SELECT:
            {
                if (!PathHelper.IsValidPath(GameDirectory))
                {
                    PathErrorMessage = "A játékkönyvtár elérési útja érvénytelen.";
                    return;
                }
                break;
            }
            case EInstallerWindow.SHORTCUTS:
            {
                if (!PathHelper.IsValidPath(StartMenuDirectory))
                {
                    PathErrorMessage = "A start menü könyvtár elérési útja érvénytelen.";
                    return;
                }
                break;
            }
            case EInstallerWindow.REVIEW:
            {
                StringBuilder reviewBuilder = new StringBuilder();
                reviewBuilder.AppendLine("Telepítési helyszín:");
                reviewBuilder.AppendLine($"  {GameDirectory}");
                reviewBuilder.AppendLine("Start menü könyvtár:");
                reviewBuilder.AppendLine($"  {StartMenuDirectory}");
                reviewBuilder.AppendLine("Parancsikonok létrehozása:");
                reviewBuilder.AppendLine($"  Asztali parancsikon: {(CreateDesktopShortcut ? "Igen" : "Nem")}");
                reviewBuilder.AppendLine($"  Start menü parancsikon: {(CreateStartMenuShortcut ? "Igen" : "Nem")}");
                reviewBuilder.AppendLine("");
                reviewBuilder.AppendLine("Figyelmeztetés:");
                reviewBuilder.AppendLine(" A telepítési helyszín könyvtára mindenképpen új könyvtár legyen, mivel a telepítő felülírhatja a meglévő fájlokat!");
                ReviewText = reviewBuilder.ToString();
                break;
            }
            case EInstallerWindow.INSTALLING:
            {
                // Start installation process
                CurrentWindow = window;
                await Task.Run(async () => await StartInstallProcessAsync());
                return;
            }
        }
        
        CurrentWindow = window;
    }
    
    /// <summary>
    /// Finalizes the installation step: optionally launches the installed game and/or opens the website, then closes the installer UI.
    /// </summary>
    [RelayCommand]
    private async Task FinishInstallation()
    {
        if (string.IsNullOrEmpty(GameDirectory))
            return;
        
        await Task.Run(() =>
        {
            try
            {
                if (LaunchGame)
                {
                    try
                    {
                        ProcessStartInfo gameLaunchInfo;
                        switch (OSHelper.GetOperatingSystem())
                        {
                            case EOperatingSystem.Windows:
                            {
                                string appPath = Path.Combine(GameDirectory, "bin",
                                    InstallHelper.GetLauncherExecutableName());
                                string appDirectory = Path.Combine(GameDirectory, "bin");
                                gameLaunchInfo = new ProcessStartInfo
                                {
                                    FileName = appPath,
                                    Arguments = "",
                                    WorkingDirectory = appDirectory,
                                    UseShellExecute = true
                                };
                                break;
                            }
                            case EOperatingSystem.Linux:
                            case EOperatingSystem.MacOS:
                            {
                                string appImagePath = Path.Combine(GameDirectory, "bin",
                                    InstallHelper.GetLauncherExecutableName());
                                string appImageDirectory = Path.Combine(GameDirectory, "bin");
                                gameLaunchInfo = new ProcessStartInfo
                                {
                                    FileName = appImagePath,
                                    Arguments = "",
                                    WorkingDirectory = appImageDirectory,
                                    UseShellExecute = true
                                };
                                break;
                            }
                            default:
                            {
                                _logger.Error("Unsupported operating system for launching the game.");
                                return Task.CompletedTask;
                            }
                        }

                        Process.Start(gameLaunchInfo);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Failed to launch the game after installation: {ex}");
                    }
                }

                if (OpenWebsite)
                    OSHelper.OpenUrl("https://mestermc.hu/");
                return Task.CompletedTask;
            }
            catch (Exception exception)
            {
                return Task.FromException(exception);
            }
        });

        await CloseInteraction.Handle(Unit.Default);
    }
    
    /// <summary>
    /// Requests the view to close by invoking the <see cref="CloseInteraction"/> interaction.
    /// </summary>
    [RelayCommand]
    private async Task CloseWindow() => await CloseInteraction.Handle(Unit.Default);
    
    /// <summary>
    /// Opens a directory picker interaction and sets <see cref="GameDirectory"/> when a value is returned.
    /// </summary>
    [RelayCommand]
    private async Task SelectGameDirectory()
    {
        var directoryResult = await DirPickerInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;
        
        if (!PathHelper.IsValidPath(directoryResult))
        {
            PathErrorMessage = "A megadott könyvtár elérési útja érvénytelen.";
            return;
        }
        
        string? rootPath = Path.GetPathRoot(directoryResult);
        if (string.IsNullOrEmpty(rootPath) || rootPath.Equals(directoryResult, StringComparison.OrdinalIgnoreCase))
        {
            PathErrorMessage = "A megadott játékkönyvtár a gyökér könyvtár, ami nem érvényes.";
            return;
        }

        if (HomeDirectory.Equals(directoryResult, StringComparison.OrdinalIgnoreCase))
        {
            PathErrorMessage = "A megadott játékkönyvtár nem lehet a 'home' könyvtár.";
            return;
        }
            
        if (DesktopDirectory.Equals(directoryResult, StringComparison.OrdinalIgnoreCase))
        {
            PathErrorMessage = "A megadott játékkönyvtár nem lehet az 'asztal' könyvtár.";
            return;
        }
            
        if (StartmenuDirectory.Equals(directoryResult, StringComparison.OrdinalIgnoreCase))
        {
            PathErrorMessage = "A megadott játékkönyvtár nem lehet a 'start menü' könyvtár.";
            return;
        }
        
        if (!FileSystemHelper.HasEnoughFreeSpace(directoryResult, 1L * 1024 * 1024 * 1024)) // Check for at least 1 GB of free space
        {
            PathErrorMessage = "A megadott könyvtárban nincs elegendő szabad hely (legalább 1 GB szükséges).";
            return;
        }
        PathErrorMessage = string.Empty;
        GameDirectory = directoryResult;
    }
    
    /// <summary>
    /// Opens a directory picker interaction and sets <see cref="StartMenuDirectory"/> when a value is returned.
    /// </summary>
    [RelayCommand]
    private async Task SelectStartMenuDirectory()
    {
        var directoryResult = await DirPickerInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;
        
        if (!PathHelper.IsValidPath(directoryResult))
        {
            PathErrorMessage = "A megadott könyvtár elérési útja érvénytelen.";
            return;
        }
        
        string? rootPath = Path.GetPathRoot(directoryResult);
        if (string.IsNullOrEmpty(rootPath) || rootPath.Equals(directoryResult, StringComparison.OrdinalIgnoreCase))
        {
            PathErrorMessage = "A megadott könyvtár a gyökér könyvtár, ami nem érvényes.";
            return;
        }
        
        if (HomeDirectory.Equals(directoryResult, StringComparison.OrdinalIgnoreCase))
        {
            PathErrorMessage = "A megadott játékkönyvtár nem lehet a 'home' könyvtár.";
            return;
        }
            
        if (DesktopDirectory.Equals(directoryResult, StringComparison.OrdinalIgnoreCase))
        {
            PathErrorMessage = "A megadott játékkönyvtár nem lehet az 'asztal' könyvtár.";
            return;
        }
        
        StartMenuDirectory = directoryResult;
    }
    #endregion
    
    /// <summary>
    /// Orchestrates the full installation process for the application.
    /// 
    /// This method performs the following high-level steps:
    /// <br/>1. Validates and normalizes input paths (<see cref="GameDirectory"/> and <see cref="StartMenuDirectory"/>).
    /// <br/>2. Verifies write permissions and free disk space.
    /// <br/>3. If an existing installation is detected, creates a backup of relevant directories and files into <c>App.TmpDir\backup.zip</c>.
    /// <br/>4. Selects the appropriate embedded launcher asset for the current OS/architecture and copies embedded resources into <c>App.TmpDir</c>.
    /// <br/>5. Extracts the launcher archive (zip or tar.gz) into a temporary extraction directory and applies the extracted files into the target <see cref="GameDirectory"/>.
    /// <br/>6. Extracts and deploys bundled configs, textures etc... (<c>content.zip</c>) into <c>{GameDirectory}\minecraftData</c>.
    /// <br/>7. Creates OS-specific shortcuts, application bundles or .desktop files and copies the application icon.
    /// <br/>8. Cleans up temporary files and, on failure, attempts to restore the backup archive.
    /// <br/>
    /// Progress and status messages are reported via <c>UpdateProgress</c> and <c>UpdateText</c>.
    /// The method performs best-effort error handling and attempts to rollback by extracting the backup on failure.
    /// </summary>
    private async Task StartInstallProcessAsync()
    {
        // Check path validity
        if (string.IsNullOrEmpty(GameDirectory) || string.IsNullOrEmpty(StartMenuDirectory))
        {
            _logger.Error("Game directory or start menu directory is null or empty.");
            UpdateText("Hiba: A könyvtár útvonalak nem lehetnek üresek.");
            return;
        }
        
        // Normalize paths
        string gameDir = Path.GetFullPath(GameDirectory);
        string startMenuDir = Path.GetFullPath(StartMenuDirectory);
        string? backupPath = null;
        bool success = false;

        UpdateProgress(0);
        UpdateText("Ellenőrzés...");
        string shortcutName = InstallHelper.GetShortcutName();
        string launcherExecutableName = InstallHelper.GetLauncherExecutableName();
        string updaterExecutableName = InstallHelper.GetUpdaterExecutableName();
        
        try
        {
            // Check write permissions
            if (!await FileSystemHelper.HasWritePermissionAsync(gameDir))
            {
                UpdateText("Hiba: Nincs írási jogosultság a megadott játékkönyvtárban.");
                _logger.Error("No write permission for the game directory.");
                return;
            }

            if (!await FileSystemHelper.HasWritePermissionAsync(startMenuDir))
            {
                UpdateText("Hiba: Nincs írási jogosultság a megadott start menü könyvtárban.");
                _logger.Error("No write permission for the start menu directory.");
                return;
            }

            if (!await FileSystemHelper.HasWritePermissionAsync(App.TmpDir))
            {
                UpdateText("Hiba: Nincs írási jogosultság az ideiglenes könyvtárban.");
                _logger.Error("No write permission for the app directory.");
                return;
            }
            
            const long requiredBytesEstimate = 500L * 1024 * 1024; 
            if (!FileSystemHelper.HasEnoughFreeSpace(App.TmpDir, requiredBytesEstimate) || !FileSystemHelper.HasEnoughFreeSpace(gameDir, requiredBytesEstimate))
            {
                UpdateText("Hiba: Nincs elegendő lemezterület a telepítéshez.");
                _logger.Error("Not enough disk space for installation.");
                return;
            }

            UpdateText("Előkészítés...");
            if (Directory.Exists(gameDir))
            {

                _logger.Warn(
                    "The target installation directory already contains an existing installation, deleting old data first.");
                UpdateText("Biztonsági mentés készítése a meglévő telepítésről...");
                backupPath = Path.Combine(App.TmpDir, "backup.zip");
                string backupTmpDir = Path.Combine(App.TmpDir, "backup");
                Directory.CreateDirectory(backupTmpDir);
                string backupDataDir = Path.Combine(backupTmpDir, "game");
                string minecraftDataDir = Path.Combine(gameDir, "game");

                // Directories to delete: minecraftData/mods, minecraftData/config, minecraftData/.fabric, minecraftData/natives
                // These directories contain data that should be removed to prevent conflicts
                try
                {
                    #region Config backup

                    UpdateText("Létező modok mentése és eltávolítása...");
                    string dirToCheck = Path.Combine(minecraftDataDir, "mods");
                    if (Directory.Exists(dirToCheck))
                        FileSystemHelper.MoveDirectory(dirToCheck, Path.Combine(backupDataDir, "mods"), true);
                    UpdateText("Létező konfigurációk mentése és eltávolítása...");
                    dirToCheck = Path.Combine(minecraftDataDir, "config");
                    if (Directory.Exists(dirToCheck))
                        FileSystemHelper.MoveDirectory(dirToCheck, Path.Combine(backupDataDir, "config"), true);
                    UpdateText("Létező Fabric fájlok mentése és eltávolítása...");
                    dirToCheck = Path.Combine(minecraftDataDir, ".fabric");
                    if (Directory.Exists(dirToCheck))
                        FileSystemHelper.MoveDirectory(dirToCheck, Path.Combine(backupDataDir, ".fabric"), true);
                    UpdateText("Létező natív fájlok mentése és eltávolítása...");
                    dirToCheck = Path.Combine(minecraftDataDir, "natives");
                    if (Directory.Exists(dirToCheck))
                        FileSystemHelper.MoveDirectory(dirToCheck, Path.Combine(backupDataDir, "natives"), true);

                    string configFilePath = Path.Combine(gameDir, "config.json");
                    if (File.Exists(configFilePath))
                    {
                        UpdateText("Létező konfigurációs fájl mentése és eltávolítása...");
                        File.Move(configFilePath, Path.Combine(backupTmpDir, "config.json"), true);
                    }

                    #endregion

                    UpdateText("Régi futattó fájlok mentése és eltávolítása...");
                    dirToCheck = Path.Combine(gameDir, "bin");
                    if (Directory.Exists(dirToCheck))
                        FileSystemHelper.MoveDirectory(dirToCheck, Path.Combine(backupTmpDir, "bin"), true);

                    UpdateText("Biztonsági mentés tömörítése...");
                    ZipFile.CreateFromDirectory(backupTmpDir, backupPath);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to delete existing installation data: {ex}");
                    UpdateText("Nem sikerült eltávolítani a létező telepítést.");
                }
            }
            else
                Directory.CreateDirectory(gameDir);

            if (CreateStartMenuShortcut && !Directory.Exists(startMenuDir))
                Directory.CreateDirectory(startMenuDir);

            string targetAssetName = string.Empty;
            bool isArm = OSHelper.IsArmBased();
            switch (OSHelper.GetOperatingSystem())
            {
                case EOperatingSystem.Windows:
                {
                    targetAssetName = isArm ? "MMCLauncher_windows_arm.zip" : "MMCLauncher_windows_x64.zip";
                    break;
                }
                case EOperatingSystem.Linux:
                {
                    targetAssetName = isArm ? "MMCLauncher_linux_arm.tar.gz" : "MMCLauncher_linux_x64.tar.gz";
                    break;
                }
                case EOperatingSystem.MacOS:
                {
                    targetAssetName = isArm ? "MMCLauncher_mac_arm.tar.gz" : "MMCLauncher_mac_x64.tar.gz";
                    break;
                }
            }

            UpdateText("Fájlok másolása...");
            UpdateProgress(20);
            string targetFilePath = Path.Combine(App.TmpDir, targetAssetName);
            var assembly = this.GetType().Assembly;
            var launcherStream = assembly
                .GetManifestResourceStream($"Tavstal.MesterMC.Installer.Software.{targetAssetName}");
            if (launcherStream == null)
            {
                _logger.Error($"Failed to get resource stream for the application zip.");
                UpdateText("Nem sikerült kimásolni az indító fájlokat.");
                return;
            }

            await using (FileStream launcherStreamOutFile = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write))
                await launcherStream.CopyToAsync(launcherStreamOutFile);

            string targetModsPath = Path.Combine(App.TmpDir, "content.zip");
            var targetModsPathStream = assembly
                .GetManifestResourceStream($"Tavstal.MesterMC.Installer.Software.content.zip");
            if (targetModsPathStream == null)
            {
                _logger.Error($"Failed to get resource stream for the mods zip.");
                UpdateText("Nem sikerült kimásolni a mod fájlokat.");
                return;
            }

            await using (FileStream targetModsStreamOutFile = new FileStream(targetModsPath, FileMode.Create, FileAccess.Write))
                await targetModsPathStream.CopyToAsync(targetModsStreamOutFile);

            //  Extract the downloaded file to the temporary directory
            UpdateText("Kicsomagolás...");
            UpdateProgress(40);
            string targetTempDir = Path.Combine(App.TmpDir, "extracted");
            if (targetAssetName.EndsWith(".tar.gz"))
            {
                await using Stream inStream = File.OpenRead(targetFilePath);
                await using Stream gzipStream = new GZipInputStream(inStream);
                using TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                tarArchive.ExtractContents(targetTempDir);
            }
            else
            {
                await using var stream = File.OpenRead(targetFilePath);
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
                archive.ExtractToDirectory(targetTempDir, true);
            }

            // Move the extracted files to the application directory
            UpdateText("Alkalmazás...");
            UpdateProgress(60);
            // Extract mods
            string modsDir = Path.Combine(gameDir, "game");
            await using (var targetModsStream = File.OpenRead(targetModsPath))
            {
                using var targetModsArchive = new ZipArchive(targetModsStream, ZipArchiveMode.Read);
                targetModsArchive.ExtractToDirectory(modsDir, true);
            }

            string binDirPath = Path.Combine(gameDir, "bin");
            if (!Directory.Exists(binDirPath))
                Directory.CreateDirectory(binDirPath);
            FileSystemHelper.MoveDirectory(targetTempDir, binDirPath, true);

            // Create shortcuts
            UpdateProgress(80);
            switch (OSHelper.GetOperatingSystem())
            {
                case EOperatingSystem.Windows:
                {
                    // Copy .ico file to bin directory
                    var stream = assembly
                        .GetManifestResourceStream("Tavstal.MesterMC.Installer.Assets.icons.favicon.ico");
                    if (stream == null)
                    {
                        _logger.Error($"Failed to get resource stream for the application icon.");
                        InstallText = "Nem sikerült kimásolni az alkalmazás ikonját.";
                        return;
                    }

                    string iconPath = Path.Combine(binDirPath, "favicon.ico");
                    await using (FileStream outFile = new FileStream(iconPath, FileMode.Create, FileAccess.Write))
                        await stream.CopyToAsync(outFile);

                    // Create main shortcut
                    string exePath = Path.Combine(binDirPath, launcherExecutableName);
                    string shortcutPath = Path.Combine(gameDir, shortcutName);
                    Shortcut.CreateShortcut(exePath, "", binDirPath, iconPath, 0).WriteToFile(shortcutPath);
                    exePath = Path.Combine(binDirPath, updaterExecutableName);
                    Shortcut.CreateShortcut(exePath, "--uninstall-prepare", OSHelper.GetDesktopDirectory(), iconPath, 0).WriteToFile(Path.Combine(gameDir, "Uninstall.lnk"));

                    // Copy to desktop if needed
                    if (CreateDesktopShortcut)
                        File.Copy(shortcutPath, Path.Combine(OSHelper.GetDesktopDirectory(), shortcutName), true);

                    if (CreateStartMenuShortcut && !string.IsNullOrEmpty(startMenuDir))
                        File.Copy(shortcutPath, Path.Combine(startMenuDir, shortcutName), true);

                    break;
                }
                case EOperatingSystem.MacOS:
                {
                    // Copy .icns file to bin directory
                    var stream = assembly
                        .GetManifestResourceStream("Tavstal.MesterMC.Installer.Assets.icons.favicon.icns");
                    if (stream == null)
                    {
                        _logger.Error($"Failed to get resource stream for the application icon.");
                        InstallText = "Nem sikerült kimásolni az alkalmazás ikonját.";
                        return;
                    }

                    string iconPath = Path.Combine(binDirPath, "favicon.icns");
                    await using (FileStream outFile = new FileStream(iconPath, FileMode.Create, FileAccess.Write))
                        await stream.CopyToAsync(outFile);

                    // Create app bundle
                    string appPath = Path.Combine(gameDir, launcherExecutableName);
                    StringBuilder appCode = new StringBuilder();
                    appCode.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    appCode.AppendLine(
                        "<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
                    appCode.AppendLine("<plist version=\"1.0\">");
                    appCode.AppendLine(" <dict>");
                    appCode.AppendLine("  <key>CFBundleExecutable</key>");
                    appCode.AppendLine("  <string>MMC-Launcher</string>");
                    appCode.AppendLine("  <key>CFBundleIconFile</key>");
                    appCode.AppendLine("  <string>favicon.icns</string>");
                    appCode.AppendLine("  <key>CFBundleIdentifier</key>");
                    appCode.AppendLine("  <string>io.github.tavstal.mestermc</string>");
                    appCode.AppendLine("  <key>CFBundleName</key>");
                    appCode.AppendLine("  <string>MesterMC</string>");
                    appCode.AppendLine("  <key>CFBundlePackageType</key>");
                    appCode.AppendLine("  <string>APPL</string>");
                    appCode.AppendLine("  <key>CFBundleShortVersionString</key>");
                    appCode.AppendLine("  <string>1.0.0</string>");
                    appCode.AppendLine("  <key>CFBundleVersion</key>");
                    appCode.AppendLine("  <string>1.0.0</string>");
                    appCode.AppendLine("  <key>LSMinimumSystemVersion</key>");
                    appCode.AppendLine("  <string>10.9.0</string>");
                    appCode.AppendLine("  <key>NSHighResolutionCapable</key>");
                    appCode.AppendLine("  <true/>");
                    appCode.AppendLine("  <key>NSPrincipalClass</key>");
                    appCode.AppendLine("  <string>NSApplication</string>");
                    appCode.AppendLine("  <key>LSApplicationCategoryType</key>");
                    appCode.AppendLine("  <string>public.app-category.games</string>");
                    appCode.AppendLine(" </dict>");
                    appCode.AppendLine("</plist>");

                    Directory.CreateDirectory(appPath);
                    string contentsPath = Path.Combine(appPath, "Contents");
                    Directory.CreateDirectory(contentsPath);
                    string macOSPath = Path.Combine(contentsPath, "MacOS");
                    Directory.CreateDirectory(macOSPath);
                    string resourcesPath = Path.Combine(contentsPath, "Resources");
                    Directory.CreateDirectory(resourcesPath);
                    await File.WriteAllTextAsync(Path.Combine(contentsPath, "Info.plist"), appCode.ToString());
                    File.Copy(Path.Combine(binDirPath, launcherExecutableName), Path.Combine(macOSPath,launcherExecutableName), true);
                    File.Copy(iconPath, Path.Combine(resourcesPath, "favicon.icns"), true);

                    // Create symlink
                    if (CreateDesktopShortcut)
                    {
                        var target = Path.Combine(OSHelper.GetDesktopDirectory(), shortcutName);
                        if (FileSystemHelper.DeleteFile(target))
                            File.CreateSymbolicLink(target, appPath);
                    }

                    if (CreateStartMenuShortcut && !string.IsNullOrEmpty(startMenuDir))
                    {
                        var target = Path.Combine(startMenuDir, shortcutName);
                        if (FileSystemHelper.DeleteFile(target))
                            File.CreateSymbolicLink(target, appPath);
                    }
                    
                    // TODO: Create uninstaller script

                    break;
                }
                default:
                {
                    // Copy .png file to bin directory
                    var stream = assembly
                        .GetManifestResourceStream("Tavstal.MesterMC.Installer.Assets.icons.favicon.png");
                    if (stream == null)
                    {
                        _logger.Error("Failed to get resource stream for the application icon.");
                        InstallText = "Nem sikerült kimásolni az alkalmazás ikonját.";
                        return;
                    }

                    string icon = Path.Combine(binDirPath, "favicon.png");
                    await using (FileStream outFile = new FileStream(icon, FileMode.Create, FileAccess.Write))
                        await stream.CopyToAsync(outFile);

                    string appPath = Path.Combine(binDirPath, launcherExecutableName);

                    foreach (var file in Directory.GetFiles(binDirPath))
                        if (file.Contains("MMC-Launcher") || file.Contains("MMC-Updater"))
                            await FileSystemHelper.MakeExecutableAsync(file);

                    // Create .desktop file
                    StringBuilder desktopFile = new StringBuilder();
                    desktopFile.AppendLine("[Desktop Entry]");
                    desktopFile.AppendLine("Name=MesterMC");
                    desktopFile.AppendLine("Comment=A MesterMC hivatalos indítója.");
                    desktopFile.AppendLine($"Exec={appPath}");
                    desktopFile.AppendLine($"Icon={icon}");
                    desktopFile.AppendLine($"Path={binDirPath}");
                    desktopFile.AppendLine("Terminal=false");
                    desktopFile.AppendLine("Type=Application");
                    desktopFile.AppendLine("Categories=Game;");
                    desktopFile.AppendLine("Keywords=Minecraft;MesterMC;Launcher;Game;");
                    desktopFile.AppendLine("StartupNotify=true");
                    string desktopFilePath = Path.Combine(gameDir, shortcutName);
                    await File.WriteAllTextAsync(desktopFilePath, desktopFile.ToString());
                    await FileSystemHelper.MakeExecutableAsync(desktopFilePath);

                    // Create symlink
                    if (CreateDesktopShortcut)
                    {
                        var target = Path.Combine(OSHelper.GetDesktopDirectory(), shortcutName);
                        if (FileSystemHelper.DeleteFile(target))
                            File.CreateSymbolicLink(target, desktopFilePath);
                    }

                    if (CreateStartMenuShortcut && !string.IsNullOrEmpty(startMenuDir))
                    {
                        var target = Path.Combine(startMenuDir, shortcutName);
                        if (FileSystemHelper.DeleteFile(target))
                            File.CreateSymbolicLink(target, desktopFilePath);
                    }
                    
                    // TODO: Create uninstaller script

                    break;
                }
            }

            // Final: Delete the temporary directory
            UpdateText("Tisztítás...");
            UpdateProgress(100);
            success = true;
        }
        catch (Exception ex)
        {
            UpdateText("Nem várt hiba a telepítés során.");
            UpdateProgress(0);
            _logger.Error(ex.ToString());
        }
        finally
        {
            if (!success && !string.IsNullOrEmpty(backupPath) && File.Exists(backupPath))
                ZipFile.ExtractToDirectory(backupPath, gameDir, true);
            
            FileSystemHelper.DeleteDirectory(App.TmpDir);
            await InstallHelper.SaveInstallPathAsync(gameDir, startMenuDir ?? " ", "0.0.4"); // TODO: Make this dynamic
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentWindow = success ? EInstallerWindow.FINISHED : EInstallerWindow.ERROR;
            });
        }
    }

    /// <summary>
    /// Updates the installer status text on the UI thread by assigning to <c>InstallText</c>.
    /// </summary>
    private void UpdateText(string text)
    {
        Dispatcher.UIThread.Post(() =>
        {
            InstallText = text;
        });
    }
    
    /// <summary>
    /// Updates the installer progress value on the UI thread by assigning to <c>InstallProgress</c>.
    /// </summary>
    private void UpdateProgress(double progress)
    {
        Dispatcher.UIThread.Post(() =>
        {
            InstallProgress = progress;
        });
    }
}