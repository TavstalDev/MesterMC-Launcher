/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.MesterMC.Launcher.Models.Config.Java;

/// <summary>
/// Represents a Java mirror with download URLs for different architectures.
/// </summary>
public class JavaMirrorArchitecture
{
    /// <summary>
    /// Gets or sets the download URL for the x86_64 architecture.
    /// </summary>
    [JsonProperty("x86_64"), JsonPropertyName("x86_64")]
    public string X86_64 { get; set; }

    /// <summary>
    /// Gets or sets the download URL for the ARM architecture.
    /// </summary>
    [JsonProperty("arm"), JsonPropertyName("arm")]
    public string Arm { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirrorArchitecture"/> class with default values.
    /// </summary>
    public JavaMirrorArchitecture()
    {
        X86_64 = string.Empty;
        Arm = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaMirrorArchitecture"/> class with specified values.
    /// </summary>
    /// <param name="x8664">The download URL for the x86_64 architecture.</param>
    /// <param name="arm">The download URL for the ARM architecture.</param>
    public JavaMirrorArchitecture(string x8664, string arm)
    {
        X86_64 = x8664;
        Arm = arm;
    }
}