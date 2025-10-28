/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Represents the details of a specific version of the Minecraft launcher.
/// </summary>
public class VersionDetails
{
    /// <summary>
    /// Gets or sets the Minecraft version associated with this version detail.
    /// </summary>
    public string MinecraftVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom version identifier for this version detail.
    /// </summary>
    public string CustomVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory where the version files are stored.
    /// </summary>
    public string VersionDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the version's JSON file.
    /// </summary>
    public string VersionJsonPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the version's JAR file.
    /// </summary>
    public string VersionJarPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the vanilla JAR file for this version.
    /// </summary>
    public string VanillaJarPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory where the game files are stored.
    /// </summary>
    public string GameDir { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the directory where native libraries are extracted.
    /// </summary>
    public string NativesDir { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionDetails"/> class.
    /// </summary>
    public VersionDetails()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionDetails"/> class with specified parameters.
    /// </summary>
    /// <param name="minecraftVersion">The Minecraft version.</param>
    /// <param name="customVersion">The custom version identifier.</param>
    /// <param name="versionDirectory">The directory where version files are stored.</param>
    /// <param name="versionJsonPath">The path to the version's JSON file.</param>
    /// <param name="versionJarPath">The path to the version's JAR file.</param>
    /// <param name="vanillaJarPath">The path to the vanilla JAR file.</param>
    /// <param name="gameDir">The directory where game files are stored.</param>
    /// <param name="nativesDir">The directory where native libraries are extracted.</param>
    public VersionDetails(string minecraftVersion, string customVersion, string versionDirectory,
        string versionJsonPath, string versionJarPath, string vanillaJarPath, string gameDir, string nativesDir)
    {
        MinecraftVersion = minecraftVersion;
        CustomVersion = customVersion;
        VersionDirectory = versionDirectory;
        VersionJsonPath = versionJsonPath;
        VersionJarPath = versionJarPath;
        VanillaJarPath = vanillaJarPath;
        GameDir = gameDir;
        NativesDir = nativesDir;
    }
}