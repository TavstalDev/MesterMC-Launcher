/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods and properties for managing application paths.
/// </summary>
public static class PathHelper
{
    private static string? _workingDirectory;
    
    /// <summary>
    /// Gets the application directory path.
    /// In debug mode, it appends "LauncherDebug" to the current directory.
    /// </summary>
    public static string ApplicationDir
    {
        get
        {
            if (!string.IsNullOrEmpty(_workingDirectory))
                return _workingDirectory;
            
#if DEBUG
            _workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "LauncherDebug");
            return _workingDirectory;
#else
            var dir = Directory.GetCurrentDirectory();
            string? dirName = Path.GetDirectoryName(dir);
            if (string.IsNullOrEmpty(dirName))
            {
                _workingDirectory = dir;
                return _workingDirectory;
            }
            
            if (!dirName.EndsWith("bin", StringComparison.OrdinalIgnoreCase))
            {
                _workingDirectory = dir.EndsWith("bin", StringComparison.OrdinalIgnoreCase) ? dirName : dir;
                return _workingDirectory;
            }

            _workingDirectory = Path.GetDirectoryName(dirName) ?? dirName;
            return _workingDirectory;
#endif
        }
    }

    /// <summary>
    /// Gets the path to the launcher configuration file.
    /// </summary>
    public static readonly string LauncherConfigPath = Path.Combine(ApplicationDir, "config.json");
    
    /// <summary>
    /// Gets the path to the Java mirrors configuration file.
    /// </summary>
    public static readonly string JavaMirrorsPath = Path.Combine(ApplicationDir, "java-mirrors.json");
    
    /// <summary>
    /// Gets the path to the launcher logs directory.
    /// </summary>
    public static readonly string LauncherLogsDir = Path.Combine(ApplicationDir, "logs");
    
    /// <summary>
    /// Specifies the format for log file names, where `{0}` is replaced with the log name.
    /// </summary>
    public static readonly string LogsFileFormat = "{0:yyyy-MM-dd_HH-mm-ss}.log";
    
    /// <summary>
    /// Validates whether the specified path is a valid file system path.
    /// </summary>
    /// <param name="path">The path to validate.</param>
    /// <returns>
    /// <c>true</c> if the path is valid; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsValidPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }
}