/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Models.Installer;

/// <summary>
/// Represents the details of various paths used in the launcher.
/// </summary>
public class PathDetails
{
    /// <summary>
    /// Gets or sets the directory path for assets.
    /// </summary>
    public string AssetsDir { get; set; }
    
    /// <summary>
    /// Gets or sets the directory path for cached files.
    /// </summary>
    public string CacheDir { get; set; }
    
    /// <summary>
    /// Gets or sets the directory path for libraries.
    /// </summary>
    public string LibrariesDir { get; set; }
    
    /// <summary>
    /// Gets or sets the directory path for versions.
    /// </summary>
    public string VersionsDir { get; set; }
    
    /// <summary>
    /// Gets or sets the path to the manifest file.
    /// </summary>
    public string ManifestPath { get; set; }
    
    /// <summary>
    /// Gets or sets the path to a custom manifest file, if any.
    /// </summary>
    public string? CustomManifestPath { get; set; }
    
    /// <summary>
    /// Gets or sets the list of custom native files.
    /// </summary>
    public List<string> CustomNativeFiles { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PathDetails"/> class with the specified parameters.
    /// </summary>
    /// <param name="assetsDir">The directory path for assets.</param>
    /// <param name="cacheDir">The directory path for cached files.</param>
    /// <param name="librariesDir">The directory path for libraries.</param>
    /// <param name="versionsDir">The directory path for versions.</param>
    /// <param name="manifestPath">The path to the manifest file.</param>
    /// <param name="customManifestPath">The path to a custom manifest file, if any.</param>
    /// <param name="customNativeFiles">The list of custom native files.</param>
    public PathDetails(string assetsDir, string cacheDir, string librariesDir, string versionsDir, string manifestPath, string? customManifestPath, List<string> customNativeFiles)
    {
        AssetsDir = assetsDir;
        CacheDir = cacheDir;
        LibrariesDir = librariesDir;
        VersionsDir = versionsDir;
        ManifestPath = manifestPath;
        CustomManifestPath = customManifestPath;
        CustomNativeFiles = customNativeFiles;
    }
}