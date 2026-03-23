using Tavstal.KonkordLauncher.Core.Enums;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Helper methods for saving, removing and retrieving the install/start-menu paths
/// across supported operating systems (Windows, Linux, macOS).
/// </summary>
public static class InstallHelper
{
    /// <summary>
    /// Save install and start menu paths for the current OS.
    /// On Windows writes values to the registry, on Linux/macOS writes a small file in the user's home.
    /// </summary>
    /// <param name="installPath">Full path to the installation directory.</param>
    /// <param name="startMenuPath">Path to the start menu / shortcuts folder.</param>
    /// <param name="appVersion">Application version used for Windows registry registration.</param>
    public static async Task SaveInstallPathAsync(string installPath, string startMenuPath, string appVersion)
    {
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                WindowsRegistryHelper.RegisterApp(installPath, startMenuPath, appVersion);
                break;
            }
            case EOperatingSystem.Linux:
            {
                string path = GetLinuxFilePath();
                if (string.IsNullOrEmpty(path))
                    return;
                
                if (File.Exists(path))
                    File.Delete(path);
                
                await File.WriteAllLinesAsync(path, [installPath, startMenuPath]);
                break;
            }
            case EOperatingSystem.MacOS:
            {
                string path = GetMacOSFilePath();
                if (string.IsNullOrEmpty(path))
                    return;
                
                if (File.Exists(path))
                    File.Delete(path);
                
                await File.WriteAllLinesAsync(path, [installPath, startMenuPath]);
                break;
            }
        }
    }

    /// <summary>
    /// Remove saved install/start-menu information for the current OS.
    /// On Windows removes registry entries; on Linux/macOS deletes the marker file.
    /// </summary>
    public static void RemoveInstallPath()
    {
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                WindowsRegistryHelper.UnregisterApp();
                break;
            }
            case EOperatingSystem.Linux:
            {
                string path = GetLinuxFilePath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return;
                File.Delete(path);
                break;
            }
            case EOperatingSystem.MacOS:
            {
                string path = GetMacOSFilePath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return;
                File.Delete(path);
                break;
            }
        }
    }
    
    /// <summary>
    /// Get the saved install path for the current OS, if present.
    /// </summary>
    public static string? GetInstallPath()
    {
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
                return WindowsRegistryHelper.GetInstalledPath();
            case EOperatingSystem.Linux:
            {
                string path = GetLinuxFilePath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return null;

                var result = File.ReadAllLines(path);
                if (result.Length < 2)
                    return null;
                
                return result.FirstOrDefault();
            }
            case EOperatingSystem.MacOS:
            {
                string path = GetMacOSFilePath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return null;

                var result = File.ReadAllLines(path);
                if (result.Length < 2)
                    return null;
                
                return result.FirstOrDefault();
            }
            default:
                return null;
        }
    }

    /// <summary>
    /// Get the saved start-menu / shortcut folder path for the current OS, if present.
    /// </summary>
    public static string? GetStartMenuPath()
    {
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
                return WindowsRegistryHelper.GetStartMenuPath();
            case EOperatingSystem.Linux:
            {
                string path = GetLinuxFilePath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return null;

                var result = File.ReadAllLines(path);
                if (result.Length < 2)
                    return null;
                
                return result[1];
            }
            case EOperatingSystem.MacOS:
            {
                string path = GetMacOSFilePath();
                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                    return null;

                var result = File.ReadAllLines(path);
                if (result.Length < 2)
                    return null;
                
                return result[1];
            }
            default:
                return null;
        }
    }

    /// <summary>
    /// Get the shortcut filename used by the platform (e.g. .lnk, .desktop, .app).
    /// </summary>
    public static string GetShortcutName()
    {
        string shortcutName = string.Empty;
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
                shortcutName = "MesterMC.lnk";
                break;
            case EOperatingSystem.Linux:
                shortcutName = "MesterMC.desktop";
                break;
            case EOperatingSystem.MacOS:
                shortcutName = "MesterMC.app";
                break;
        }
        return shortcutName;
    }
    
    /// <summary>
    /// Get the launcher executable filename for the current OS.
    /// </summary>
    public static string GetLauncherExecutableName()
    {
        string shortcutName = string.Empty;
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
                shortcutName = "MMC-Launcher.exe";
                break;
            case EOperatingSystem.Linux:
            case EOperatingSystem.MacOS:
                shortcutName = "MMC-Launcher";
                break;
        }
        return shortcutName;
    }
    
    /// <summary>
    /// Get the updater executable filename for the current OS.
    /// </summary>
    public static string GetUpdaterExecutableName()
    {
        string shortcutName = string.Empty;
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
                shortcutName = "MMC-Updater.exe";
                break;
            case EOperatingSystem.Linux:
            case EOperatingSystem.MacOS:
                shortcutName = "MMC-Updater";
                break;
        }
        return shortcutName;
    }
    
    /// <summary>
    /// Path to the small marker file used on Linux to store install and start menu paths.
    /// </summary>
    private static string GetLinuxFilePath()
    {
        string homeDir = OSHelper.GetHomeDirectory();
        return Path.Combine(homeDir, ".config", ".mmc-installer.txt");
    }

    /// <summary>
    /// Path to the small marker file used on macOS to store install and start menu paths.
    /// </summary>
    private static string GetMacOSFilePath()
    {
        string homeDir = OSHelper.GetHomeDirectory();
        return Path.Combine(homeDir, "Library", "Preferences", ".mmc-installer.txt");
    }
}