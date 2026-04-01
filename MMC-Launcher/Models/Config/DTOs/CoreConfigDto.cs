/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.MesterMC.Launcher.Models.Config.DTOs;

/// <summary>
/// Represents the core configuration for the launcher, including launcher settings, Java settings,
/// Minecraft settings, and miscellaneous options.
/// </summary>
[RequiresUnreferencedCode("This class uses code that may be removed during trimming.")]
public class CoreConfigDto
{
    /// <summary>
    /// Gets or sets the configuration for the launcher.
    /// </summary>
    [JsonProperty("launcher"), JsonPropertyName("launcher")]
    public LauncherConfigDto Launcher { get; set; }
    
    /// <summary>
    /// Gets or sets the Java configuration for the launcher.
    /// </summary>
    [JsonProperty("java"), JsonPropertyName("java")]
    public JavaConfigDto Java { get; set; }
    
    /// <summary>
    /// Gets or sets the Minecraft configuration for the launcher.
    /// </summary>
    [JsonProperty("minecraft"), JsonPropertyName("minecraft")]
    public MinecraftConfigDto Minecraft { get; set; }
    
    /// <summary>
    /// Gets or sets the miscellaneous configuration for the launcher.
    /// </summary>
    [JsonProperty("misc"), JsonPropertyName("misc")]
    public MiscConfigDto Misc { get; set; }
    
    /// <summary>
    /// The UTC date and time when cached data was last refreshed.
    /// </summary>
    [JsonProperty("cacheRefreshDate"), JsonPropertyName("cacheRefreshDate")]
    public DateTime CacheRefreshDate { get; set; }
    
    /// <summary>
    /// Index or identifier of the last user who played/used the launcher.
    /// </summary>
    [JsonProperty("lastPlayed"), JsonPropertyName("lastPlayed")]
    public int LastUser { get; set; }
    
    /// <summary>
    /// Mapping of user identifiers to a display name or identifier used by the launcher.
    /// </summary>
    [JsonProperty("users"), JsonPropertyName("users")]
    public Dictionary<string, string> Users { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfigDto"/> class with default values.
    /// </summary>
    public CoreConfigDto()
    {
        Launcher = new LauncherConfigDto();
        Java = new JavaConfigDto();
        Minecraft = new MinecraftConfigDto();
        Misc = new MiscConfigDto();
        CacheRefreshDate = DateTime.UtcNow;
        LastUser = -1;
        Users = new Dictionary<string, string>();
    }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfigDto"/> class using the provided values.
    /// </summary>
    /// <param name="launcher">The launcher configuration DTO to assign to the <see cref="Launcher"/> property.</param>
    /// <param name="java">The Java configuration DTO to assign to the <see cref="Java"/> property.</param>
    /// <param name="minecraft">The Minecraft configuration DTO to assign to the <see cref="Minecraft"/> property.</param>
    /// <param name="misc">The miscellaneous configuration DTO to assign to the <see cref="Misc"/> property.</param>
    /// <param name="cacheRefreshDate">The date and time when cached data was last refreshed (UTC).</param>
    /// <param name="lastUser">Index or identifier of the last user who played; -1 typically indicates none.</param>
    /// <param name="users">A dictionary mapping user indices/ids to a display name or identifier; may be null or empty.</param>
    public CoreConfigDto(LauncherConfigDto launcher, JavaConfigDto java, MinecraftConfigDto minecraft, MiscConfigDto misc, DateTime cacheRefreshDate, int lastUser, Dictionary<string, string> users)
    {
        Launcher = launcher;
        Java = java;
        Minecraft = minecraft;
        Misc = misc;
        CacheRefreshDate = cacheRefreshDate;
        LastUser = lastUser;
        Users = users;
    }
}