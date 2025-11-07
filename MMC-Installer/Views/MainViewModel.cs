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

public partial class MainViewModel : ObservableObject
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainViewModel));
    public readonly string TmpDir;
    
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

    public MainViewModel()
    {
        TmpDir = Path.Combine(Path.GetTempPath(), "mmcupdater_" + Path.GetRandomFileName());
        if (!Directory.Exists(TmpDir))
            Directory.CreateDirectory(TmpDir);
    }
    
    [RelayCommand]
    private async Task OpenWindow(EInstallerWindow window)
    {
        switch (window)
        {
            case EInstallerWindow.LocationSelect:
            {
                if (!PathHelper.IsValidPath(GameDirectory))
                {
                    PathErrorMessage = "A játékkönyvtár elérési útja érvénytelen.";
                    return;
                }
                break;
            }
            case EInstallerWindow.Shortcuts:
            {
                if (!PathHelper.IsValidPath(StartMenuDirectory))
                {
                    PathErrorMessage = "A start menü könyvtár elérési útja érvénytelen.";
                    return;
                }
                break;
            }
            case EInstallerWindow.Review:
            {
                StringBuilder reviewBuilder = new StringBuilder();
                reviewBuilder.AppendLine("Telepítési helyszín:");
                reviewBuilder.AppendLine($"  {GameDirectory}");
                reviewBuilder.AppendLine("Start menü könyvtár:");
                reviewBuilder.AppendLine($"  {StartMenuDirectory}");
                reviewBuilder.AppendLine("Parancsikonok létrehozása:");
                reviewBuilder.AppendLine($"  Asztali parancsikon: {(CreateDesktopShortcut ? "Igen" : "Nem")}");
                reviewBuilder.AppendLine($"  Start menü parancsikon: {(CreateStartMenuShortcut ? "Igen" : "Nem")}");
                ReviewText = reviewBuilder.ToString();
                break;
            }
            case EInstallerWindow.Installing:
            {
                // Start installation process
                CurrentWindow = window;
                await Dispatcher.UIThread.InvokeAsync(async () => await StartInstallProcessAsync());
                return;
            }
        }
        
        CurrentWindow = window;
    }
    
    [RelayCommand]
    private async Task FinishInstallation()
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
                        string appPath = Path.Combine(GameDirectory, "bin", "MMC-Launcher.exe");
                        string appDirectory = Path.Combine(GameDirectory, "bin");
                        gameLaunchInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = $"/C start \"\" \"{appPath}\" /D \"{appDirectory}\"",
                            WorkingDirectory = appDirectory,
                            UseShellExecute = false 
                        };
                        break;
                    }
                    case EOperatingSystem.Linux:
                    {
                        string appImagePath = Path.Combine(GameDirectory, "bin", "MMC-Launcher");
                        string appImageDirectory = Path.Combine(GameDirectory, "bin"); 
                        gameLaunchInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"nohup {appImagePath} > /dev/null 2>&1 &\"",
                            WorkingDirectory = appImageDirectory, 
                            UseShellExecute = false
                        };
                        break;
                    }
                    case EOperatingSystem.MacOS:
                    {
                        string appImagePath = Path.Combine(GameDirectory, "bin", "MMC-Launcher");
                        string appImageDirectory = Path.Combine(GameDirectory, "bin"); 
                        gameLaunchInfo = new ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = $"-c \"nohup {appImagePath} > /dev/null 2>&1 &\"",
                            WorkingDirectory = appImageDirectory, 
                            UseShellExecute = false
                        };
                        break;
                    }
                    default:
                    {
                        _logger.Error("Unsupported operating system for launching the game.");
                        return;
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

        await Task.Delay(500); // Small delay to ensure processes start before closing
        await CloseInteraction.Handle(Unit.Default);
    }
    
    [RelayCommand]
    private async Task CloseWindow()
    {
        await CloseInteraction.Handle(Unit.Default);
    }
    
    [RelayCommand]
    private async Task SelectGameDirectory()
    {
        var directoryResult = await DirPickerInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;
        
        GameDirectory = directoryResult;
    }
    
    [RelayCommand]
    private async Task SelectStartMenuDirectory()
    {
        var directoryResult = await DirPickerInteraction.Handle(Unit.Default);
        if (string.IsNullOrEmpty(directoryResult))
            return;
        
        StartMenuDirectory = directoryResult;
    }
    
    private async Task StartInstallProcessAsync()
    {
        // TODO: Test on all platforms
        InstallText = "Előkészítés...";
        if (!Directory.Exists(GameDirectory))
            Directory.CreateDirectory(GameDirectory);
        
        if (CreateStartMenuShortcut && !Directory.Exists(StartMenuDirectory))
            Directory.CreateDirectory(StartMenuDirectory);
        
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
        
        InstallText = "Fájlok másolása...";
        string targetFilePath = Path.Combine(TmpDir, targetAssetName);
        var launcherStream = this.GetType().Assembly.GetManifestResourceStream($"Tavstal.MesterMC.Installer.Software.{targetAssetName}");
        if (launcherStream == null)
        {
            _logger.Error($"Failed to get resource stream for the application zip.");
            InstallText = "Nem sikerült kimásolni az indító fájlokat.";
            return;
        }
        
        await using FileStream launcherStreamOutFile = new FileStream(targetFilePath, FileMode.Create, FileAccess.Write);
        await launcherStream.CopyToAsync(launcherStreamOutFile);
        
        string targetModsPath = Path.Combine(TmpDir, "content.zip");
        var targetModsPathStream = this.GetType().Assembly.GetManifestResourceStream($"Tavstal.MesterMC.Installer.Software.content.zip");
        if (targetModsPathStream == null)
        {
            _logger.Error($"Failed to get resource stream for the mods zip.");
            InstallText = "Nem sikerült kimásolni a mod fájlokat.";
            return;
        }
        
        await using FileStream targetModsStreamOutFile = new FileStream(targetModsPath, FileMode.Create, FileAccess.Write);
        await targetModsPathStream.CopyToAsync(targetModsStreamOutFile);
        
        //  Extract the downloaded file to the temporary directory
        InstallText = "Kicsomagolás...";
        string targetTempDir = Path.Combine(TmpDir, "extracted");
        if (targetAssetName.EndsWith(".tar.gz"))
        {
            await using Stream inStream = File.OpenRead(targetFilePath);
            await using Stream gzipStream = new GZipInputStream(inStream);
            using TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            tarArchive.ExtractContents(targetTempDir);
        }
        else
            ZipFile.ExtractToDirectory(targetFilePath, targetTempDir, true);

        // 3. Move the extracted files to the application directory
        InstallText = "Alkalmazás...";
        // Extract mods
        string modsDir = Path.Combine(GameDirectory, "minecraftData");
        ZipFile.ExtractToDirectory(targetModsPath, modsDir, true);
        
        string binDirhPath = Path.Combine(GameDirectory, "bin");
        if (!Directory.Exists(binDirhPath))
            Directory.CreateDirectory(binDirhPath);
        FileSystemHelper.MoveDirectory(targetTempDir, binDirhPath, true);

        // 4. Create shortcuts
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                // Copy .ico file to bin directory
                var stream = this.GetType().Assembly.GetManifestResourceStream("Tavstal.MesterMC.Installer.Assets.icons.favicon.ico");
                if (stream == null)
                {
                    _logger.Error($"Failed to get resource stream for the application icon.");
                    InstallText = "Nem sikerült kimásolni az alkalmazás ikonját.";
                    return;
                }

                string iconPath = Path.Combine(binDirhPath, "favicon.ico");
                await using FileStream outFile = new FileStream(iconPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(outFile);
                
                // Create main shortcut
                string exePath = Path.Combine(binDirhPath, "MMC-Launcher.exe");
                string shortcutPath = Path.Combine(GameDirectory, "MesterMC.lnk");
                Shortcut.CreateShortcut(exePath, "", binDirhPath, iconPath, 0).WriteToFile(shortcutPath);
                
                foreach (var file in Directory.GetFiles(binDirhPath))
                {
                    if (file.Contains("MMC-Launcher") || file.Contains("MMC-Updater"))
                        await FileSystemHelper.MakeExecutableAsync(file);
                }
                
                // Copy to desktop if needed
                if (CreateDesktopShortcut)
                    File.Copy(shortcutPath, Path.Combine(OSHelper.GetDesktopDirectory(), "MesterMC.lnk"), true);
                
                if (CreateStartMenuShortcut && !string.IsNullOrEmpty(StartMenuDirectory))
                    File.Copy(shortcutPath, Path.Combine(StartMenuDirectory, "MesterMC.lnk"), true);
                
                break;
            }
            case EOperatingSystem.MacOS:
            {
                // Copy .icns file to bin directory
                var stream = this.GetType().Assembly.GetManifestResourceStream("Tavstal.MesterMC.Installer.Assets.icons.favicon.icns");
                if (stream == null)
                {
                    _logger.Error($"Failed to get resource stream for the application icon.");
                    InstallText = "Nem sikerült kimásolni az alkalmazás ikonját.";
                    return;
                }

                string iconPath = Path.Combine(binDirhPath, "favicon.icns");
                await using FileStream outFile = new FileStream(iconPath, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(outFile);
                
                // Create app bundle
                string appPath = Path.Combine(GameDirectory, "MMC-Launcher.app");
                StringBuilder appCode = new StringBuilder();
                appCode.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                appCode.AppendLine("<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">");
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
                File.Copy(Path.Combine(binDirhPath, "MMC-Launcher"), Path.Combine(macOSPath, "MMC-Launcher"), true);
                File.Copy(iconPath, Path.Combine(resourcesPath, "favicon.icns"), true);
                
                // Create symlink
                if (CreateDesktopShortcut)
                {
                    var target = Path.Combine(OSHelper.GetDesktopDirectory(), "MesterMC");
                    if (File.Exists(target))
                        File.Delete(target);
                    File.CreateSymbolicLink(target, appPath);
                }

                if (CreateStartMenuShortcut && !string.IsNullOrEmpty(StartMenuDirectory))
                {
                    var target = Path.Combine(StartMenuDirectory, "MesterMC");
                    if (File.Exists(target))
                        File.Delete(target);
                    File.CreateSymbolicLink(target, appPath);
                }
                
                break;
            }
            default:
            {
                // Copy .png file to bin directory
                var stream = this.GetType().Assembly.GetManifestResourceStream("Tavstal.MesterMC.Installer.Assets.icons.favicon.png");
                if (stream == null)
                {
                    _logger.Error("Failed to get resource stream for the application icon.");
                    InstallText = "Nem sikerült kimásolni az alkalmazás ikonját.";
                    return;
                }

                string icon = Path.Combine(binDirhPath, "favicon.png");
                await using FileStream outFile = new FileStream(icon, FileMode.Create, FileAccess.Write);
                await stream.CopyToAsync(outFile);
                
                string appPath = Path.Combine(binDirhPath, "MMC-Launcher");

                foreach (var file in Directory.GetFiles(binDirhPath))
                {
                    if (file.Contains("MMC-Launcher") || file.Contains("MMC-Updater"))
                        await FileSystemHelper.MakeExecutableAsync(file);
                }
                
                // Create .desktop file
                StringBuilder desktopFile = new StringBuilder();
                desktopFile.AppendLine("[Desktop Entry]");
                desktopFile.AppendLine("Name=MesterMC");
                desktopFile.AppendLine("Comment=A MesterMC hivatalos indítója.");
                desktopFile.AppendLine($"Exec={appPath}");
                desktopFile.AppendLine($"Icon={icon}");
                desktopFile.AppendLine($"Path={binDirhPath}");
                desktopFile.AppendLine("Terminal=false");
                desktopFile.AppendLine("Type=Application");
                desktopFile.AppendLine("Categories=Game;");
                desktopFile.AppendLine("Keywords=Minecraft;MesterMC;Launcher;Game;");
                desktopFile.AppendLine("StartupNotify=true");
                string desktopFilePath = Path.Combine(GameDirectory, "MesterMC.desktop");
                await File.WriteAllTextAsync(desktopFilePath, desktopFile.ToString());
                await FileSystemHelper.MakeExecutableAsync(desktopFilePath);
                
                // Create symlink
                if (CreateDesktopShortcut)
                {
                    var target = Path.Combine(OSHelper.GetDesktopDirectory(), "MesterMC.desktop");
                    if (File.Exists(target))
                        File.Delete(target);
                    File.CreateSymbolicLink(target, desktopFilePath);
                }

                if (CreateStartMenuShortcut && !string.IsNullOrEmpty(StartMenuDirectory))
                {
                    var target = Path.Combine(StartMenuDirectory, "MesterMC.desktop");
                    if (File.Exists(target))
                        File.Delete(target);
                    File.CreateSymbolicLink(target, desktopFilePath);
                }

                break;
            }
        }
        
        // Final: Delete the temporary directory
        InstallText = "Tisztítás...";
        if (Directory.Exists(TmpDir))
            FileSystemHelper.DeleteDirectory(TmpDir);
        
        CurrentWindow = EInstallerWindow.Finished;
    }
}