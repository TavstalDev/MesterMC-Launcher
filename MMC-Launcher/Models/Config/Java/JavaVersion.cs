/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.MesterMC.Launcher.Models.Config.Java;

/// <summary>
/// Represents a Java version with its major version, full version string, architecture, and installation path.
/// </summary>
public class JavaVersion
{
    /// <summary>
    /// Gets or sets the major version of Java.
    /// </summary>
    public int Major { get; set; }

    /// <summary>
    /// Gets or sets the full version string of Java.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// Gets or sets the architecture of the Java installation (e.g., x86, x64).
    /// </summary>
    public string Architecture { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the Java installation.
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaVersion"/> class.
    /// </summary>
    public JavaVersion() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaVersion"/> class with the specified properties.
    /// </summary>
    /// <param name="major">The major version of Java.</param>
    /// <param name="version">The full version string of Java.</param>
    /// <param name="architecture">The architecture of the Java installation.</param>
    /// <param name="path">The file system path to the Java installation.</param>
    public JavaVersion(int major, string version, string architecture, string path)
    {
        Major = major;
        Version = version;
        Architecture = architecture;
        Path = path;
    }
}