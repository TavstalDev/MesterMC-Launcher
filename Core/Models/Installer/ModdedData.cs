/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

namespace Tavstal.KonkordLauncher.Core.Models.Installer;

/// <summary>
/// Represents the modded data required for the installation process.
/// </summary>
public class ModdedData
{
    /// <summary>
    /// Gets or sets the main class of the modded data.
    /// </summary>
    public string? MainClass { get; set; }

    /// <summary>
    /// Gets or sets the version details associated with the modded data.
    /// </summary>
    public VersionDetails VersionData { get; set; }

    /// <summary>
    /// Gets or sets the list of libraries required for the modded data.
    /// </summary>
    public List<LibraryMeta> Libraries { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedData"/> class.
    /// </summary>
    public ModdedData() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ModdedData"/> class with specified main class, version details, and libraries.
    /// </summary>
    /// <param name="mainClass">The main class of the modded data.</param>
    /// <param name="versionData">The version details associated with the modded data.</param>
    /// <param name="libraries">The list of libraries required for the modded data.</param>
    public ModdedData(string? mainClass, VersionDetails versionData, List<LibraryMeta> libraries)
    {
        MainClass = mainClass;
        VersionData = versionData;
        Libraries = libraries;
    }
}