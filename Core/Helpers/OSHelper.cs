/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Diagnostics;
using System.Runtime.InteropServices;
using Hardware.Info;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for operating system-related functionality.
/// </summary>
public static class OSHelper
{
    private static CoreLogger _logger = CoreLogger.WithModuleType(typeof(OSHelper));
    
    /// <summary>
    /// Determines the operating system type.
    /// </summary>
    /// <returns>
    /// An <see cref="EOperatingSystem"/> value representing the current operating system.
    /// </returns>
    public static EOperatingSystem GetOperatingSystem()
    {
        var platform = Environment.OSVersion.Platform;
        switch (platform)
        {
            case PlatformID.Win32NT:
            case PlatformID.Win32Windows:
            case PlatformID.Win32S:
            case PlatformID.WinCE:
            {
                return EOperatingSystem.Windows;
            }
            case PlatformID.Unix:
            {
                return EOperatingSystem.Linux;
            }
            case PlatformID.MacOSX:
            {
                return EOperatingSystem.MacOS;
            }
            default:
            {
                return EOperatingSystem.Unknown;
            }
        }
    }
    
    /// <summary>
    /// Determines if the operating system is Windows 11.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the operating system is Windows 11.
    /// </returns>
    public static bool IsWIndows11()
    {
        if (GetOperatingSystem() != EOperatingSystem.Windows)
            return false;

        Version osVersion = Environment.OSVersion.Version;
        return (osVersion.Major > 10) || osVersion is { Major: 10, Build: >= 22000 };
    }

    /// <summary>
    /// Determines if the operating system is ARM-based.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the operating system architecture is ARM
    /// </returns>
    public static bool IsArmBased()
    {
        Architecture osArchitecture = RuntimeInformation.OSArchitecture;
        return osArchitecture == Architecture.Arm || osArchitecture == Architecture.Arm64;
    }

    /// <summary>
    /// Checks if the operating system is 64-bit.
    /// </summary>
    /// <returns>
    /// A boolean value indicating whether the operating system is 64-bit.
    /// </returns>
    public static bool Is64BitOperatingSystem()
    {
        return Environment.Is64BitOperatingSystem;
    }
    
    /// <summary>
    /// Retrieves the type and description of the dedicated GPU available on the system.
    /// </summary>
    /// <returns>
    /// A tuple containing:
    /// - A string representing the GPU type ("nvidia", "amd", "intel", or "apple") if detected.
    /// - A string representing the GPU description.
    /// Returns <c>null</c> if no dedicated GPU is detected.
    /// </returns>
    public static (string, string)? GetDedicatedGpuType()
    {
        var hardwareInfo = new HardwareInfo();
        hardwareInfo.RefreshVideoControllerList();
        foreach (var gpu in hardwareInfo.VideoControllerList)
        {
            var lowerName = gpu.Description.ToLowerInvariant();
            if (lowerName.Contains("nvidia") && (lowerName.Contains("geforce") || 
                                                 lowerName.Contains("quadro") || 
                                                 lowerName.Contains("gtx") || 
                                                 lowerName.Contains("rtx") || 
                                                 lowerName.Contains("mx")))
                return ("nvidia", gpu.Description);
            
            if (lowerName.Contains("radeon rx") || lowerName.Contains("radeon r9") || lowerName.Contains("radeon r7") || lowerName.Contains("radeon r5"))
                return ("amd", gpu.Description);
            
            if (lowerName.Contains("intel arc") || lowerName.Contains("intel battlemage"))
                return ("intel", gpu.Description);
            
            if (lowerName.Contains("apple"))
                return ("apple", gpu.Description);
        }
        
        return null;
    }

    /// <summary>
    /// Retrieves the total amount of physical RAM available on the system in bytes.
    /// </summary>
    /// <returns>
    /// A <see cref="ulong"/> value representing the total physical memory in bytes.
    /// </returns>
    public static ulong GetRamInBytes()
    {
        HardwareInfo hardwareInfo = new HardwareInfo();
        hardwareInfo.RefreshMemoryStatus();

        return hardwareInfo.MemoryStatus.TotalPhysical;
    }
    
    /// <summary>
    /// Retrieves the home directory path for the current user.
    /// </summary>
    /// <param name="os">
    /// Optional parameter specifying the operating system. If null, the current operating system is detected.
    /// </param>
    /// <returns>
    /// A string representing the path to the user's home directory.
    /// </returns>
    public static string GetHomeDirectory(EOperatingSystem? os = null)
    {
        os ??= GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.Windows:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            }
            case EOperatingSystem.Linux:
            case EOperatingSystem.MacOS:
            case EOperatingSystem.Unknown:
            {
                return Environment.GetEnvironmentVariable("HOME") ?? string.Empty;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(os));
        }
    }
    
    /// <summary>
    /// Retrieves the desktop directory path for the current user.
    /// </summary>
    /// <param name="os">
    /// Optional parameter specifying the operating system. If null, the current operating system is detected.
    /// </param>
    /// <returns>
    /// A string representing the path to the user's desktop directory.
    /// </returns>
    public static string GetDesktopDirectory(EOperatingSystem? os = null)
    {
        os ??= GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.Windows:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            case EOperatingSystem.MacOS:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            }
            case EOperatingSystem.Linux:
            case EOperatingSystem.Unknown:
            {
                var xdgDesktop = Environment.GetEnvironmentVariable("XDG_DESKTOP_DIR");
                if (!string.IsNullOrEmpty(xdgDesktop))
                    return xdgDesktop;
                
                string userHomeDir = GetHomeDirectory();
                string desktopDir = Path.Combine(userHomeDir, "Desktop"); // Fallback to "Desktop" in home directory
                if (Directory.Exists(desktopDir))
                    return desktopDir;
                
                var userDirsFilePath = Path.Combine(userHomeDir, ".config", "user-dirs.dirs");
                if (!File.Exists(userDirsFilePath))
                    return desktopDir; 
                
                string[] fileContent =  File.ReadAllLines(userDirsFilePath);
                foreach (string line in fileContent)
                {
                    if (!line.StartsWith("XDG_DESKTOP_DIR="))
                        continue;
                    
                    desktopDir = line.Split('=')[1].Trim('"');
                    break;
                }
                return  desktopDir;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(os));
        }
    }
    
    /// <summary>
    /// Retrieves the programs directory path for the current user.
    /// </summary>
    /// <param name="os">
    /// Optional parameter specifying the operating system. If null, the current operating system is detected.
    /// </param>
    /// <returns>
    /// A string representing the path to the user's programs directory.
    /// </returns>
    public static string GetProgramsDirectory(EOperatingSystem? os = null)
    {
        os ??= GetOperatingSystem();

        switch (os)
        {
            case EOperatingSystem.Windows:
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            }
            case EOperatingSystem.MacOS:
            {
                // MacOS: Standard directory for applications.
                return "/Applications";
            }
            case EOperatingSystem.Linux:
            case EOperatingSystem.Unknown:
            {
                // Linux: Standard directory for user-specific applications.
                // ~/.local/share/applications
                string userHomeDir = GetHomeDirectory();
                return Path.Combine(userHomeDir, ".local", "share", "applications"); 
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(os));
        }
    }
    
    /// <summary>
    /// Opens the specified URL in the default web browser based on the operating system.
    /// </summary>
    /// <param name="url">The URL to be opened.</param>
    public static void OpenUrl(string url)
    {
        try
        {
            ProcessStartInfo startInfo;
            switch (GetOperatingSystem())
            {
                case EOperatingSystem.Windows:
                {
                    startInfo = new ProcessStartInfo(url)
                    {
                        UseShellExecute = true
                    };
                    break;
                }
                case EOperatingSystem.MacOS:
                {
                    startInfo = new ProcessStartInfo("open", url)
                    {
                        UseShellExecute = false
                    };
                    break;
                }
                case EOperatingSystem.Linux:
                {
                    startInfo = new ProcessStartInfo("xdg-open", url)
                    {
                        UseShellExecute = false // xdg-open is the executable
                    };
                    break;
                }
                default:
                {
                    _logger.Warn("Unsupported operating system for opening URLs.");
                    return;
                }
            }
        
            // Start the process
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            _logger.Error($"Failed to open the website after installation: {ex.Message}");
        }
    }

    /// <summary>
    /// Collects hardware information about the system, including operating system, CPU, RAM, disk size, and GPU.
    /// </summary>
    /// <returns>
    /// An object containing the following hardware details:
    /// - <c>os</c>: The operating system as a string.
    /// - <c>cpu</c>: The CPU identifier retrieved from the environment variable.
    /// - <c>ram</c>: The total physical memory in gigabytes.
    /// - <c>disksize</c>: The total disk size in gigabytes.
    /// - <c>gpu</c>: The description of the first detected GPU, or "unknown" if no GPU is found.
    /// </returns>
    public static object CollectHardwareInfo()
    {
        var hardwareInfo = new HardwareInfo();
        hardwareInfo.RefreshVideoControllerList();
        hardwareInfo.RefreshMemoryList();
        hardwareInfo.RefreshMemoryStatus();
        hardwareInfo.RefreshDriveList();
        
        ulong totalDiskSize = 0;
        foreach (var drive in hardwareInfo.DriveList)
        {
            totalDiskSize += drive.Size;
        }
        
        string gpu = "unknown";
        foreach (var videoController in hardwareInfo.VideoControllerList)
        {
            gpu = videoController.Description;
            break;
        }
        
        return new {
            os = GetOperatingSystem().ToString(),
            cpu = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER"),
            ram = hardwareInfo.MemoryStatus.TotalPhysical / (1024 * 1024 * 1024),
            disksize = totalDiskSize / (1024L*1024*1024),
            gpu
        };
    }
}