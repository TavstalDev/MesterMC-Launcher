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
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

/// <summary>
/// Represents a classifier for native libraries, including platform-specific artifacts for Windows, macOS, and Linux.
/// </summary>
public class Classifier
{
    /// <summary>
    /// Gets or sets the artifact for Windows native libraries.
    /// </summary>
    [JsonPropertyName("natives-windows"), JsonProperty("natives-windows")]
    public Artifact WindowsNatives { get; set; }

    /// <summary>
    /// Gets or sets the artifact for macOS native libraries.
    /// </summary>
    [JsonPropertyName("natives-osx"), JsonProperty("natives-osx")]
    public Artifact OsxNatives { get; set; }

    /// <summary>
    /// Gets or sets the artifact for Linux native libraries.
    /// </summary>
    [JsonPropertyName("natives-linux"), JsonProperty("natives-linux")]
    public Artifact LinuxNatives { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Classifier"/> class.
    /// </summary>
    public Classifier()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Classifier"/> class with specified platform-specific artifacts.
    /// </summary>
    /// <param name="windowsNatives">The artifact for Windows native libraries.</param>
    /// <param name="osxNatives">The artifact for macOS native libraries.</param>
    /// <param name="linuxNatives">The artifact for Linux native libraries.</param>
    public Classifier(Artifact windowsNatives, Artifact osxNatives, Artifact linuxNatives)
    {
        WindowsNatives = windowsNatives;
        OsxNatives = osxNatives;
        LinuxNatives = linuxNatives;
    }

    /// <summary>
    /// Retrieves the native library artifact for the current operating system.
    /// </summary>
    /// <returns>
    /// The <see cref="Artifact"/> object corresponding to the native library for the detected operating system.
    /// </returns>
    public Artifact GetOsNative()
    {
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                return WindowsNatives;
            }
            case EOperatingSystem.MacOS:
            {
                return OsxNatives;
            }
            default:
            case EOperatingSystem.Linux:
            case EOperatingSystem.Unknown:
            {
                return LinuxNatives;
            }
        }
    }
}