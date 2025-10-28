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

namespace Tavstal.KonkordLauncher.Common.Models.Config;

/// <summary>
/// Represents the core configuration for the launcher, including launcher settings, Java settings,
/// Minecraft settings, and miscellaneous options.
/// </summary>
public class CoreConfig
{
    /// <summary>
    /// Gets or sets the configuration for the launcher.
    /// </summary>
    [JsonProperty("launcher"), JsonPropertyName("launcher")]
    public LauncherConfig Launcher { get; set; }
    
    /// <summary>
    /// Gets or sets the Java configuration for the launcher.
    /// </summary>
    [JsonProperty("java"), JsonPropertyName("java")]
    public JavaConfig Java { get; set; }
    
    /// <summary>
    /// Gets or sets the Minecraft configuration for the launcher.
    /// </summary>
    [JsonProperty("minecraft"), JsonPropertyName("minecraft")]
    public MinecraftConfig Minecraft { get; set; }
    
    /// <summary>
    /// Gets or sets the miscellaneous configuration for the launcher.
    /// </summary>
    [JsonProperty("misc"), JsonPropertyName("misc")]
    public MiscConfig Misc { get; set; }
    
    [JsonProperty("cacheRefreshDate"), JsonPropertyName("cacheRefreshDate")]
    public DateTime CacheRefreshDate { get; set; }
    
    [JsonProperty("enableEnvironmentVariables"), JsonPropertyName("enableEnvironmentVariables")]
    public bool EnableEnvironmentVariables { get; set; }
    
    [JsonProperty("environmentVariables"), JsonPropertyName("environmentVariables")]
    public Dictionary<string, string> EnvironmentVariables { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfig"/> class with default values.
    /// </summary>
    public CoreConfig()
    {
        Launcher = new LauncherConfig();
        Java = new JavaConfig()
        {
            JvmArguments = Instance.GetDefaultJVMArgs()
        };
        Minecraft = new MinecraftConfig();
        Misc = new MiscConfig();
        CacheRefreshDate = DateTime.Now;
        EnableEnvironmentVariables = false;
        EnvironmentVariables = new Dictionary<string, string>();
    }
    
    public CoreConfig(LauncherConfig launcher, JavaConfig java, MinecraftConfig minecraft, MiscConfig misc, DateTime cacheRefreshDate, bool enableEnvironmentVariables, Dictionary<string, string> environmentVariables)
    {
        Launcher = launcher;
        Java = java;
        Minecraft = minecraft;
        Misc = misc;
        CacheRefreshDate = cacheRefreshDate;
        EnableEnvironmentVariables = enableEnvironmentVariables;
        EnvironmentVariables = environmentVariables;
    }
}