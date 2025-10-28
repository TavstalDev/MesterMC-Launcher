/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Common.Models.Config;

/// <summary>
/// Represents the configuration settings for the launcher, including update settings, 
/// language, theme, and directory paths.
/// </summary>
public class LauncherConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether automatic updates are enabled.
    /// </summary>
    [JsonProperty("enableAutomaticUpdates"), JsonPropertyName("enableAutomaticUpdates")]
    public bool EnableAutomaticUpdates { get; set; }
    
    /// <summary>
    /// Gets or sets the update interval in hours.
    /// </summary>
    [JsonProperty("updateInterval"), JsonPropertyName("updateInterval")]
    public uint UpdateInterval { get; set; }

    /// <summary>
    /// Gets or sets the date and time for the next update check.
    /// </summary>
    [JsonProperty("nextUpdateCheck"), JsonPropertyName("nextUpdateCheck")]
    public DateTime NextUpdateCheck { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the assets directory.
    /// </summary>
    [JsonProperty("assetsDirectoryPath"), JsonPropertyName("assetsDirectoryPath")]
    public string AssetsDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the cache directory.
    /// </summary>
    [JsonProperty("cacheDirectoryPath"), JsonPropertyName("cacheDirectoryPath")]
    public string CacheDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the icons directory.
    /// </summary>
    [JsonProperty("iconsDirectoryPath"), JsonPropertyName("iconsDirectoryPath")]
    public string IconsDirectoryPath { get; set; }
    
    [JsonProperty("minecraftDirectoryPath"), JsonPropertyName("minecraftDirectoryPath")]
    public string MinecraftDataDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the Java directory.
    /// </summary>
    [JsonProperty("javaDirectoryPath"), JsonPropertyName("javaDirectoryPath")]
    public string JavaDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the libraries directory.
    /// </summary>
    [JsonProperty("librariesDirectoryPath"), JsonPropertyName("librariesDirectoryPath")]
    public string LibrariesDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the manifests directory.
    /// </summary>
    [JsonProperty("manifestsDirectoryPath"), JsonPropertyName("manifestsDirectoryPath")]
    public string ManifestsDirectoryPath { get; set; }
    
    /// <summary>
    /// Gets or sets the file system path to the versions directory.
    /// </summary>
    [JsonProperty("versionsDirectoryPath"), JsonPropertyName("versionsDirectoryPath")]
    public string VersionsDirectoryPath { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherConfig"/> class with default values.
    /// </summary>
    public LauncherConfig()
    {
        EnableAutomaticUpdates = true;
        UpdateInterval = 24; // Default to 24 hours
        NextUpdateCheck = DateTime.MinValue; // Default to never checked
        AssetsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "assets");
        CacheDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "cache");
        IconsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "icons");
        MinecraftDataDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "minecraftData");
        JavaDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "java");
        LibrariesDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "libraries");
        ManifestsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "manifests");
        VersionsDirectoryPath = Path.Combine(PathHelper.ApplicationDir, "versions");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherConfig"/> class with specified values.
    /// </summary>
    /// <param name="enableAutomaticUpdates">Indicates whether automatic updates are enabled.</param>
    /// <param name="updateInterval">The update interval in hours.</param>
    /// <param name="nextUpdateCheck">The date and time for the next update check.</param>
    /// <param name="assetsDirectoryPath">The file system path to the assets directory.</param>
    /// <param name="cacheDirectoryPath">The file system path to the cache directory.</param>
    /// <param name="iconsDirectoryPath">The file system path to the icons directory.</param>
    /// <param name="minecraftDataDirectoryPath">The file system path to the instances directory.</param>
    /// <param name="javaDirectoryPath">The file system path to the Java directory.</param>
    /// <param name="librariesDirectoryPath">The file system path to the libraries directory.</param>
    /// <param name="manifestsDirectoryPath">The file system path to the manifests directory.</param>
    /// <param name="versionsDirectoryPath">The file system path to the versions directory.</param>
    public LauncherConfig(bool enableAutomaticUpdates, uint updateInterval, DateTime nextUpdateCheck, string assetsDirectoryPath, string cacheDirectoryPath, string iconsDirectoryPath, string minecraftDataDirectoryPath, string javaDirectoryPath, string librariesDirectoryPath, string manifestsDirectoryPath, string versionsDirectoryPath)
    {
        EnableAutomaticUpdates = enableAutomaticUpdates;
        UpdateInterval = updateInterval;
        NextUpdateCheck = nextUpdateCheck;
        AssetsDirectoryPath = assetsDirectoryPath;
        CacheDirectoryPath = cacheDirectoryPath;
        IconsDirectoryPath = iconsDirectoryPath;
        MinecraftDataDirectoryPath = minecraftDataDirectoryPath;
        JavaDirectoryPath = javaDirectoryPath;
        LibrariesDirectoryPath = librariesDirectoryPath;
        ManifestsDirectoryPath = manifestsDirectoryPath;
        VersionsDirectoryPath = versionsDirectoryPath;
    }
    
    /// <summary>
    /// Gets the file system path to the vanilla manifest file.
    /// </summary>
    /// <returns>The path to the vanilla manifest file.</returns>
    public string GetVanillaManifestPath()
    {
        return Path.Combine(ManifestsDirectoryPath, "vanillaManifest.json");
    }
}