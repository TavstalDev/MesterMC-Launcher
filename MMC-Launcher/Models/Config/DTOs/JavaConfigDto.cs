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

namespace Tavstal.MesterMC.Launcher.Models.Config.DTOs;

/// <summary>
/// Represents the configuration for Java settings, including memory allocation, Java path, and JVM arguments.
/// </summary>
public class JavaConfigDto
{
    /// <summary>
    /// Gets or sets the minimum memory allocation for the Java process, in megabytes.
    /// </summary>
    [JsonProperty("minMemory"), JsonPropertyName("minMemory")]
    public uint MinMemory { get; set; }

    /// <summary>
    /// Gets or sets the maximum memory allocation for the Java process, in megabytes.
    /// </summary>
    [JsonProperty("maxMemory"), JsonPropertyName("maxMemory")]
    public uint MaxMemory { get; set; }

    /// <summary>
    /// Gets or sets the size of the permanent generation (PermGen) memory, in megabytes.
    /// </summary>
    [JsonProperty("permaGen"), JsonPropertyName("permaGen")]
    public uint PermaGen { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaConfigDto"/> class with default values.
    /// </summary>
    public JavaConfigDto()
    {
        MinMemory = 1024;
        MaxMemory = 4096;
        PermaGen = 128;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaConfigDto"/> class with specified values.
    /// </summary>
    /// <param name="minMemory">The minimum memory allocation for the Java process, in megabytes.</param>
    /// <param name="maxMemory">The maximum memory allocation for the Java process, in megabytes.</param>
    /// <param name="permaGen">The size of the permanent generation (PermGen) memory, in megabytes.</param>
    public JavaConfigDto(uint minMemory, uint maxMemory, uint permaGen)
    {
        MinMemory = minMemory;
        MaxMemory = maxMemory;
        PermaGen = permaGen;
    }
}