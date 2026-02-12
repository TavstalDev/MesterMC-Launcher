/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using NbtLib;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;

namespace Tavstal.KonkordLauncher.Common.Helpers;

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

            if (!Directory.Exists(settings.Launcher.VersionsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.VersionsDirectoryPath);

            if (!Directory.Exists(settings.Launcher.CacheDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.CacheDirectoryPath);

            if (!Directory.Exists(settings.Launcher.LibrariesDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.LibrariesDirectoryPath);

            if (!Directory.Exists(settings.Launcher.AssetsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.AssetsDirectoryPath);

            string indexes = Path.Combine(settings.Launcher.AssetsDirectoryPath, "indexes");
            if (!Directory.Exists(indexes))
                Directory.CreateDirectory(indexes);

            if (!Directory.Exists(settings.Launcher.ManifestsDirectoryPath))
                Directory.CreateDirectory(settings.Launcher.ManifestsDirectoryPath);

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
            using var httpClient = new HttpClient();
            var settings = await LauncherHelper.GetLauncherSettingsAsync();
            bool refreshManifests = DateTimeOffset.UtcNow > settings.CacheRefreshDate;
            
            // Vanilla
            if (!File.Exists(settings.Launcher.GetVanillaManifestPath()) || refreshManifests)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.SetStatus("A minecraft manifest letöltése... {0}%", e.ToString("0.00"));
                };
                await HttpHelper.DownloadFileAsync(MicrosoftEndpoints.MinecraftManifestUrl, settings.Launcher.GetVanillaManifestPath(), progress);
            }
            progressReporter?.SetStatus("A minecraft manifest ellenőrzése...");
            if (await ManifestHelper.GetMinecraftManifestAsync(settings.Launcher.GetVanillaManifestPath()) == null)
                _logger.Error("Failed to load Minecraft manifest");
            
            // Fabric
            if (!File.Exists(settings.Launcher.GetFabricManifestPath()) || refreshManifests)
            {
                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, e) =>
                {
                    progressReporter?.SetStatus("A {0} fabric manifest letöltése... {1}%", "fabric", e.ToString("0.00"));
                };
                await HttpHelper.DownloadFileAsync(FabricEndpoints.VersionManifestUrl, settings.Launcher.GetFabricManifestPath(), progress);
            }
            progressReporter?.SetStatus("A fabric manifest ellenőrzése...");
            if (await ManifestHelper.GetFabricManifestAsync(settings.Launcher.GetFabricManifestPath()) == null)
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

            // Define a default server entry
            var serverTag = new NbtCompoundTag
            {
                { "name", new NbtStringTag("MesterMC Hub") }, // Server name
                { "ip", new NbtStringTag("play.mestermc.hu") }, // Server IP address
                { "acceptTextures", new NbtIntTag(1) }, // Accept textures flag
            };

            // Add the server entry to the servers list
            serversList.Add(serverTag);
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