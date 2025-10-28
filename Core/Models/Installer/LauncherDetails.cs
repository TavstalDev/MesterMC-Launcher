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
/// Represents the details of a launcher, including its name and version.
/// </summary>
public class LauncherDetails
{
    /// <summary>
    /// Gets or sets the name of the launcher.
    /// </summary>
    public string LauncherName { get; set; }

    /// <summary>
    /// Gets or sets the version of the launcher.
    /// </summary>
    public string LauncherVersion { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LauncherDetails"/> class with the specified name and version.
    /// </summary>
    /// <param name="launcherName">The name of the launcher.</param>
    /// <param name="launcherVersion">The version of the launcher.</param>
    public LauncherDetails(string launcherName, string launcherVersion)
    {
        LauncherName = launcherName;
        LauncherVersion = launcherVersion;
    }
}