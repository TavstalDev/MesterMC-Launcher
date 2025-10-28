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

namespace Tavstal.KonkordLauncher.Common.Models.InstanceConfig;

/// <summary>
/// Represents the configuration for custom commands executed at different stages of a game instance lifecycle.
/// </summary>
public class InstanceCommandsConfig
{
    /// <summary>
    /// Gets or sets the command to be executed before the game launches.
    /// </summary>
    [JsonProperty("preLaunchCommand"), JsonPropertyName("preLaunchCommand")]
    public string PreLaunchCommand { get; set; }

    /// <summary>
    /// Gets or sets the wrapper command to be executed during the game runtime.
    /// </summary>
    [JsonProperty("wrapperCommand"), JsonPropertyName("wrapperCommand")]
    public string WrapperCommand { get; set; }

    /// <summary>
    /// Gets or sets the command to be executed after the game exits.
    /// </summary>
    [JsonProperty("postExitCommand"), JsonPropertyName("postExitCommand")]
    public string PostExitCommand { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceCommandsConfig"/> class with default values.
    /// </summary>
    public InstanceCommandsConfig()
    {
        PreLaunchCommand = string.Empty;
        WrapperCommand = string.Empty;
        PostExitCommand = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceCommandsConfig"/> class with specified values.
    /// </summary>
    /// <param name="preLaunchCommand">The command to be executed before the game launches.</param>
    /// <param name="wrapperCommand">The wrapper command to be executed during the game runtime.</param>
    /// <param name="postExitCommand">The command to be executed after the game exits.</param>
    public InstanceCommandsConfig(string preLaunchCommand, string wrapperCommand, string postExitCommand)
    {
        PreLaunchCommand = preLaunchCommand;
        WrapperCommand = wrapperCommand;
        PostExitCommand = postExitCommand;
    }
}