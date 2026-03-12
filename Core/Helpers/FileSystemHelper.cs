/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Diagnostics;
using System.Security.Cryptography;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for file system operations such as deleting, moving directories, and verifying file hashes.
/// </summary>
public static class FileSystemHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(FileSystemHelper));

    /// <summary>
    /// Deletes a directory and all its contents.
    /// </summary>
    /// <param name="path">The path of the directory to delete.</param>
    public static void DeleteDirectory(string path)
    {
        var forgeInstallerDirInfo = new DirectoryInfo(path);
        foreach (FileInfo file in forgeInstallerDirInfo.GetFiles())
            file.Delete();

        foreach (DirectoryInfo subDirectory in forgeInstallerDirInfo.GetDirectories())
            subDirectory.Delete(true);

        Directory.Delete(path);
    }

    /// <summary>
    /// Moves a directory and its contents to a new location.
    /// </summary>
    /// <param name="sourceDir">The source directory path.</param>
    /// <param name="destinationDir">The destination directory path.</param>
    /// <param name="recursive">Indicates whether to move subdirectories recursively.</param>
    /// <param name="deleteSource">Indicates whether to delete the source directory after moving. Default is true.</param>
    /// <param name="overwrite">Indicates whether to overwrite existing files in the destination. Default is true.</param>
    /// <exception cref="DirectoryNotFoundException">Thrown if the source directory does not exist.</exception>
    public static void MoveDirectory(string sourceDir, string destinationDir, bool recursive, bool deleteSource = true,
        bool overwrite = true)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        DirectoryInfo[] dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        // Get the files in the source directory and copy to the destination directory
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            if (overwrite || (!overwrite && !File.Exists(targetFilePath)))
                file.CopyTo(targetFilePath, true);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (recursive)
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                MoveDirectory(subDir.FullName, newDestinationDir, true, false);
            }

        if (deleteSource)
            DeleteDirectory(sourceDir);
    }
    
    /// <summary>
    /// Opens a folder in the system's default file explorer.
    /// </summary>
    /// <param name="path">The path to the folder to open.</param>
    public static void OpenFolderInFileExplorer(string path)
    {
        // Ensure the path exists
        if (!Directory.Exists(path))
        {
            _logger.Error($"Error: Folder not found at '{path}'");
            return;
        }

        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                Process.Start("explorer.exe", path);
                break;
            }
            case EOperatingSystem.MacOS:
            {
                Process.Start("open", path);
                break;
            }
            case EOperatingSystem.Linux:
            {
                Process.Start("xdg-open", path);
                break;
            }
            case EOperatingSystem.Unknown:
            {
                _logger.Error("Error: Unsupported operating system for opening folder in file explorer.");
                break;
            }
        }
    }

    /// <summary>
    /// Verifies the SHA1 hash of a file against a given hash value.
    /// </summary>
    /// <param name="path">The path of the file to check.</param>
    /// <param name="compareHash">The SHA1 hash to compare against. If null or empty, the method returns true.</param>
    /// <returns>True if the file's hash matches the given hash; otherwise, false.</returns>
    public static bool CheckSHA1(string path, string? compareHash)
    {
        if (string.IsNullOrEmpty(compareHash))
            return true;

        try
        {
            string fileHash;
            using (FileStream file = File.OpenRead(path))
            using (SHA1 hasher = SHA1.Create())
            {
                var binaryHash = hasher.ComputeHash(file);
                fileHash = Convert.ToHexStringLower(binaryHash);
            }

            return string.Equals(fileHash, compareHash);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to check SHA1 hash:");
            _logger.Error(ex.ToString());
            return false;
        }
    }
    
    /// <summary>
    /// Verifies the SHA256 hash of a file against a given hash value.
    /// </summary>
    /// <param name="path">The path of the file to check.</param>
    /// <param name="compareHash">
    /// The SHA256 hash to compare against. If null or empty, the method returns true.
    /// </param>
    /// <returns>
    /// True if the file's hash matches the given hash; otherwise, false.
    /// </returns>
    public static bool CheckSHA256(string path, string? compareHash)
    {
        if (string.IsNullOrEmpty(compareHash))
            return true;

        try
        {
            string fileHash;
            using (FileStream file = File.OpenRead(path))
            using (SHA256 hasher = SHA256.Create())
            {
                var binaryHash = hasher.ComputeHash(file);
                fileHash = Convert.ToHexStringLower(binaryHash);
            }

            return string.Equals(fileHash, compareHash);
        }
        catch (Exception ex)
        {
            _logger.Exc("Failed to check SHA256 hash:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Verifies a file's hash using a digest string in the format "type:hash".
    /// Supported digest types are "sha1" and "sha256" (case-insensitive).
    /// </summary>
    /// <param name="path">Filesystem path to the file to verify.</param>
    /// <param name="digest">
    /// The digest string in the form "type:hash", for example "sha1:abcdef..." or "sha256:0123...".
    /// The method splits on the first ':' and treats the left part as the digest type and the right part as the hex hash.
    /// </param>
    /// <returns>
    /// <c>true</c> when the digest format is valid and the file's computed hash matches the provided hash;
    /// <c>false</c> when the digest format is invalid, the digest type is unsupported, or the hash comparison fails.
    /// </returns>
    public static bool CheckByDigest(string path, string digest)
    {
        string[] parts = digest.Split(':', 2);
        if (parts.Length < 2)
        {
            _logger.Error($"Invalid digest format: '{digest}'. Expected format is 'type:hash'.");
            return false;
        }
        
        switch (parts[0].ToLower())
        {
            case "sha1":
                return CheckSHA1(path, parts[1]);
            case "sha256":
                return CheckSHA256(path, parts[1]);
            default:
                _logger.Error($"Unsupported digest type '{parts[0]}' in digest string '{digest}'.");
                return false;
        }
    }

    /// <summary>
    /// Makes a file executable by modifying its permissions using the `chmod` command.
    /// </summary>
    /// <param name="path">The path of the file to make executable.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value:
    /// true if the operation succeeded, false otherwise.
    /// </returns>
    public static async Task<bool> MakeExecutableAsync(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
                return false;
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{path}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                string error = await process.StandardError.ReadToEndAsync();
                _logger.Exc($"Error while making '{path}' executable:");
                _logger.Error(error);
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Failed to make '{path}' executable:");
            _logger.Error(ex.ToString());
            return false;
        }
    }
    
    /// <summary>
    /// Converts a file size in bytes to a human-readable string format.
    /// </summary>
    /// <param name="size">The size of the file in bytes.</param>
    /// <returns>
    /// A string representing the file size in the most appropriate unit 
    /// (B, KB, MB, GB, or TB) with two decimal places of precision.
    /// </returns>
    public static string GetFormatedSize(long size)
    {
        if (size < 1024)
            return $"{size} B";
        if (size < 1024 * 1024)
            return $"{size / 1024.0:F2} KB";
        if (size < 1024 * 1024 * 1024)
            return $"{size / 1024.0 / 1024.0:F2} MB";
        if (size < 1024L * 1024L * 1024L * 1024L)
            return $"{size / 1024.0 / 1024.0 / 1024.0:F2} GB";
        // Hopefully we won't reach this point, but just in case
        return $"{size / 1024.0 / 1024.0 / 1024.0 / 1024.0:F2} TB";
    }

    /// <summary>
    /// Ensures the existence of the `command_history.txt` file in the specified game directory.
    /// If the file already exists, it is deleted and recreated as a read-only file.
    /// </summary>
    /// <param name="gameDir">The path to the game directory where the file should be fixed.</param>
    public static void FixCommandHistoryFile(string gameDir)
    {
        try
        {
            if (!Directory.Exists(gameDir))
                Directory.CreateDirectory(gameDir);

            string commandHistoryFilePath = Path.Combine(gameDir, "command_history.txt");
            if (File.Exists(commandHistoryFilePath))
            {
                var attribute = File.GetAttributes(commandHistoryFilePath);
                if (attribute.HasFlag(FileAttributes.ReadOnly))
                    return;
                File.Delete(commandHistoryFilePath);
            }

            File.Create(commandHistoryFilePath).Close();
            var attributes = File.GetAttributes(commandHistoryFilePath);
            attributes |= FileAttributes.ReadOnly;
            File.SetAttributes(commandHistoryFilePath, attributes);
        }
        catch (Exception e)
        {
            _logger.Exc("Failed to fix command_history.txt file:");
            _logger.Error(e.ToString());
        }
    }

    /// <summary>
    /// Tests whether the application can write to the given directory by creating a small test file.
    /// Preserves the original behavior: creates the directory if needed, creates "test.txt", verifies its existence,
    /// deletes it if created, and returns true on success; logs and returns false on any exception.
    /// </summary>
    /// <param name="targetDir">Directory to test write permissions for.</param>
    /// <returns>True if a test file could be created and detected; otherwise false.</returns>
    public static bool HasWritePermission(string targetDir)
    {
        try
        {
            Directory.CreateDirectory(targetDir);
            
            string testFile = Path.Combine(targetDir, "test.txt");
            if (File.Exists(testFile))
                File.Delete(testFile);
            
            File.WriteAllText(testFile, "test");
            bool success = File.Exists(testFile);
            if (success)
                File.Delete(testFile);
            return success;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while testing write permissions at {targetDir}:");
            _logger.Error(ex);
            return false;
        }
    }
    
    /// <summary>
    /// Asynchronously tests whether the application has write permission to the given directory by attempting
    /// to create a small test file ("test.txt") inside it.
    /// </summary>
    /// <param name="targetDir">The directory to test write permissions for. The directory will be created if it does not exist.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is <c>true</c> if the test file could be created
    /// and detected; otherwise <c>false</c>. Any exception is logged and results in <c>false</c>.
    /// </returns>
    public static async Task<bool> HasWritePermissionAsync(string targetDir)
    {
        try
        {
            Directory.CreateDirectory(targetDir);
            
            string testFile = Path.Combine(targetDir, "test.txt");
            if (File.Exists(testFile))
                File.Delete(testFile);
            
            await File.WriteAllTextAsync(testFile, "test");
            bool success = File.Exists(testFile);
            if (success)
                File.Delete(testFile);
            return success;
        }
        catch (Exception ex)
        {
            _logger.Error($"Unexpected error while testing write permissions at {targetDir}:");
            _logger.Error(ex);
            return false;
        }
    }
}