using Microsoft.Win32;

namespace Tavstal.KonkordLauncher.Core.Helpers;
#pragma warning disable CA1416 // Suppress warnings about Windows-specific APIs, as this class is intended for Windows only.

/// <summary>
/// Helper methods for registering/unregistering the application in the Windows registry
/// under the current user's "Uninstall" registry key.
/// </summary>
public static class WindowsRegistryHelper
{
    /// <summary>
    /// Registry path under HKCU where the application registration is stored.
    /// </summary>
    private const string RegPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\MesterMC";

    /// <summary>
    /// Creates or updates the uninstallation registration for the application in the current user's registry.
    /// </summary>
    /// <param name="installPath">Full path to the application's installation directory.</param>
    /// <param name="startMenuPath">Path to the application's Start Menu shortcut folder.</param>
    /// <param name="version">Display version string to show in Programs & Features.</param>
    /// <exception cref="Exception">Thrown when the registry key could not be created.</exception>
    public static void RegisterApp(string installPath, string startMenuPath, string version)
    {
        // Make sure existing key is removed
        try { UnregisterApp(); } catch (Exception) { /* Ignore errors if the key doesn't exist */ }

        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RegPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key == null)
            throw new Exception("Failed to create registry key for MesterMC Launcher.");

        
        string exePath = Path.Combine(installPath, "bin", InstallHelper.GetLauncherExecutableName());
        string updaterPath = Path.Combine(installPath, "bin", InstallHelper.GetUpdaterExecutableName());

        key.SetValue("DisplayName", "MesterMC Launcher");
        key.SetValue("DisplayVersion", version);
        key.SetValue("InstallLocation", installPath);
        key.SetValue("StartMenuLocation", startMenuPath);
        key.SetValue("DisplayIcon", exePath);
        key.SetValue("UninstallString", $"\"{updaterPath}\" --uninstall-prepare");

        key.SetValue("Publisher", "Tavstal");
        key.SetValue("Contact", "Tavstal");
        key.SetValue("HelpLink", "https://github.com/TavstalDev/MesterMC-Launcher");

        key.Flush(); // Force the update
    }
    
    /// <summary>
    /// Updates the "UninstallString" value for the application entry under the current user's
    /// </summary>
    /// <param name="uninstallString">
    /// The complete command line to store as the uninstallation command (for example: "\"C:\path\updater.exe\" --uninstall-prepare").
    /// </param>
    /// <exception cref="Exception">
    /// Thrown when the registry key cannot be opened for read/write access (for example if the key does not exist
    /// or access is denied).
    /// </exception>
    public static void UpdateUninstallString(string uninstallString)
    {
        using RegistryKey key = Registry.CurrentUser.OpenSubKey(RegPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
        if (key == null)
            throw new Exception("Failed to create registry key for MesterMC Launcher.");
        
        key.SetValue("UninstallString", uninstallString);
        key.Flush(); // Force the update
    }

    /// <summary>
    /// Removes the application's registry subtree under the current user's uninstall key.
    /// </summary>
    public static void UnregisterApp() => Registry.CurrentUser.DeleteSubKeyTree(RegPath, false);
    
    /// <summary>
    /// Reads the stored installation path from the registry, if present and valid.
    /// </summary>
    /// <returns>
    /// The install path from the registry if the value exists and the directory still exists;
    /// otherwise <c>null</c>.
    /// </returns>
    public static string? GetInstalledPath()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegPath);
        if (key == null)
            return null;

        var path = key.GetValue("InstallLocation")?.ToString();
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            return path;
        return null;
    }
    
    /// <summary>
    /// Reads the stored Start Menu shortcut folder path from the registry, if present and valid.
    /// </summary>
    /// <returns>
    /// The Start Menu path from the registry if the value exists and the directory still exists;
    /// otherwise <c>null</c>.
    /// </returns>
    public static string? GetStartMenuPath()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RegPath);
        if (key == null)
            return null;

        var path = key.GetValue("StartMenuLocation")?.ToString();
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            return path;
        return null;
    }
}