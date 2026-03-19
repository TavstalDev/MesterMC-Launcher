/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NbtLib;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.MesterMC.Launcher.Helpers;

/// <summary>
/// Provides helper methods for validating various launcher components, such as data folders, settings, accounts, and manifests.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Logger instance for the ValidationHelper module.
    /// </summary>
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ValidationHelper));

    /// <summary>
    /// Validates the existence of required data folders and creates them if they do not exist.
    /// </summary>
    /// <returns>True if all required folders are validated or created successfully, otherwise false.</returns>
    public static bool ValidateDataFolder()
    {
        try
        {
            if (!Directory.Exists(PathHelper.LauncherLogsDir))
                Directory.CreateDirectory(PathHelper.LauncherLogsDir);
            
            string logsFilePath = Path.Combine(PathHelper.LauncherLogsDir, string.Format(PathHelper.LogsFileFormat, CoreLogger.StartTime));
            if (!File.Exists(logsFilePath))
                File.Create(logsFilePath);
            
            if (!Directory.Exists(PathHelper.ApplicationDir))
                Directory.CreateDirectory(PathHelper.ApplicationDir);
            
            // Note: Also creates the config file if it does not exist
            var settings = LauncherHelper.GetLauncherSettings();
            
            if (!Directory.Exists(settings.Launcher.MinecraftDataDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.MinecraftDataDirectoryPath);
            
            if (!Directory.Exists(settings.Launcher.JavaDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.JavaDirectoryPath);

            if (!Directory.Exists(settings.Launcher.CacheDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.CacheDirectoryPath);

            if (!Directory.Exists(settings.Launcher.LibrariesDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.LibrariesDirectoryPath);

            if (!Directory.Exists(settings.Launcher.AssetsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.AssetsDirectoryPath);

            string indexes = Path.Combine(settings.Launcher.AssetsDirectoryPath, "indexes");
            if (!Directory.Exists(indexes))
                Directory.CreateDirectory(indexes);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to validate data folder:");
            _logger.Error(ex.ToString());
            return false;
        }
    }
    
    /// <summary>
    /// Validates and updates various manifests required by the launcher, such as Vanilla, Fabric, Forge, NeoForge, and Quilt.
    /// Downloads the manifests if they are missing or outdated.
    /// </summary>
    /// <param name="progressReporter">
    /// An optional progress reporter to report the download progress of the manifests.
    /// </param>
    public static async Task<bool> ValidateManifests(IProgressReporter? progressReporter = null)
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            
            // Vanilla
            var manifestSream = assembly.GetManifestResourceStream($"Tavstal.MesterMC.Launcher.Assets.manifests.vanilla.json");
            if (manifestSream == null)
            {
                _logger.Error("Failed to load manifest resource");
                return false;
            }
            progressReporter?.SetStatus("A minecraft manifest ellenőrzése...");
            if (await ManifestHelper.GetMinecraftManifestAsync(manifestSream) == null)
                _logger.Error("Failed to load Minecraft manifest");
            
            // Fabric
            manifestSream = assembly.GetManifestResourceStream($"Tavstal.MesterMC.Launcher.Assets.manifests.fabric.json");
            if (manifestSream == null)
            {
                _logger.Error("Failed to load manifest resource");
                return false;
            }
            progressReporter?.SetStatus("A fabric manifest ellenőrzése...");
            if (await ManifestHelper.GetFabricManifestAsync(manifestSream) == null)
                _logger.Error("Failed to load Fabric manifest");

            return true;
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to validate manifests:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Validate a single Java major version by delegating to the array overload.
    /// </summary>
    /// <param name="javaVersion">The Java major version to validate (e.g. 8, 11, 17, 21).</param>
    /// <param name="progressReporter">Optional progress reporter to receive status and progress updates.</param>
    /// <returns>
    /// A task that resolves to a tuple:
    /// Item1: <c>true</c> if validation (and any required downloads) succeeded, otherwise <c>false</c>.
    /// Item2: A suggested Java path (string) to persist into settings when the current configured Java path is empty; otherwise <c>null</c>.
    /// </returns>
    public static async Task<(bool, string?)> ValidateJavaAsync(int javaVersion, IProgressReporter? progressReporter = null) => await ValidateJavaAsync([javaVersion], progressReporter);

    /// <summary>
    /// Validates that at least one installation exists for each requested Java major version. If a required version is missing,
    /// downloads it into the configured Java directory. On POSIX systems sets the executable bit for the java binary.
    /// </summary>
    /// <param name="javaVersionsToValidate">Array of Java major versions to validate (e.g. { 17, 21 }).</param>
    /// <param name="progressReporter">Optional progress reporter used to surface status and progress updates to the caller/UI.</param>
    /// <returns>
    /// A task that resolves to a tuple:
    /// Item1: <c>true</c> when validation completed (even if some non-fatal steps failed); <c>false</c> when an unexpected exception occurred.
    /// Item2: When the current configured settings do not contain a Java path and one or more installations were found, returns the first discovered installation path to be saved into settings; otherwise <c>null</c>.
    /// </returns>
    public static async Task<(bool, string?)> ValidateJavaAsync(int[] javaVersionsToValidate, IProgressReporter? progressReporter = null)
    {
        try
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            var javaInstallations = JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath);
            bool wasJavaUpdated = false;
            foreach (int javaVersion in javaVersionsToValidate)
            {
                var jdkResult = javaInstallations.FirstOrDefault(x => x.Major == javaVersion);
                if (jdkResult != null)
                {
                    _logger.Info("Java installation found for version " + javaVersion + " at " + jdkResult.Path);
                    continue;
                }

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, prog) =>
                {
                    progressReporter?.SetStatus("Java " + javaVersion + " letöltése... " + prog.ToString("0.00") + "%");
                    progressReporter?.SetProgress(prog);
                };
                await JavaHelper.DownloadJavaVersionAsync(settings.Launcher.JavaDirectoryPath, progress);
                wasJavaUpdated = true;
            }

            if (wasJavaUpdated)
            {
                if (OSHelper.GetOperatingSystem() != EOperatingSystem.Windows)
                {
                    string[] directories = Directory.GetDirectories(settings.Launcher.JavaDirectoryPath);
                    foreach (string directory in directories)
                    {
                        string javaExecutablePath = Path.Combine(directory, "bin", "java");
                        if (!File.Exists(javaExecutablePath))
                            continue;
                        if (!await FileSystemHelper.MakeExecutableAsync(javaExecutablePath))
                        {
                            progressReporter?.SetStatus("Nem sikerült végrehajthatóvá tenni a Java fájlt.");
                            _logger.Error("Failed to make Java executable: " + javaExecutablePath);
                        }
                    }
                }

                javaInstallations = JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath, true);
            }

            return (true, javaInstallations[0].Path);
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to validate Java:");
            _logger.Error(ex.ToString());
            return (false, null);
        }
    }

    /// <summary>
    /// Validates the existence of the `servers.dat` file in the Minecraft data directory.
    /// If the file does not exist, it creates a default `servers.dat` file with predefined server information.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a boolean value:
    /// true if the validation or creation is successful, otherwise false.
    /// </returns>
    public static async Task<bool> ValidateServersAsync()
    {
        try
        {
            // Retrieve the launcher settings asynchronously
            var settings = await LauncherHelper.GetLauncherSettingsAsync();

            // Construct the file path for the `servers.dat` file
            string filePath = Path.Combine(settings.Launcher.MinecraftDataDirectoryPath, "servers.dat");

            // If the file already exists, return true
            if (File.Exists(filePath))
                return true;

            // Create the root NBT tag and the servers list tag
            var root = new NbtCompoundTag();
            var serversList = new NbtListTag(NbtTagType.Compound);

            // Add the server entry to the servers list
            serversList.Add(new NbtCompoundTag
            {
                { "name", new NbtStringTag("MesterMC Hub") }, // Server name
                { "ip", new NbtStringTag("play.mestermc.hu") }, // Server IP address
                { "acceptTextures", new NbtIntTag(1) }, // Accept textures flag
            });
            serversList.Add(new NbtCompoundTag
            {
                { "name", new NbtStringTag("Test Server") }, // Server name
                { "ip", new NbtStringTag("localhost") }, // Server IP address
                { "acceptTextures", new NbtIntTag(1) }, // Accept textures flag
            });
            root.Add("servers", serversList);

            // Write the NBT data to the `servers.dat` file
            await using var outputStream = new NbtWriter().CreateUncompressedNbtStream(root, "");
            await using var fileStream = File.Create(filePath);
            outputStream.Seek(0, SeekOrigin.Begin);
            await outputStream.CopyToAsync(fileStream);

            // Return true to indicate success
            return true;
        }
        catch (Exception ex)
        {
            // Log the error and return false to indicate failure
            _logger.Error("Failed to validate servers:");
            _logger.Error(ex.ToString());
            return false;
        }
    }
}