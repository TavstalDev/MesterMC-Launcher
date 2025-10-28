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
using Tavstal.KonkordLauncher.Common.Models.Config;

namespace Tavstal.KonkordLauncher.Common.Models.InstanceConfig;

/// <summary>
/// Represents the configuration for a game instance, including Java settings, game settings,
/// custom commands, environment variables, and miscellaneous options.
/// </summary>
public class InstanceConfig
{
    /// <summary>
    /// Gets or sets the Java configuration for the game instance.
    /// </summary>
    [JsonProperty("java"), JsonPropertyName("java")]
    public JavaConfig Java { get; set; }

    /// <summary>
    /// Gets or sets the game-specific configuration for the instance.
    /// </summary>
    [JsonProperty("game"), JsonPropertyName("game")]
    public InstanceGameConfig Game { get; set; }

    /// <summary>
    /// Gets or sets the custom commands configuration for the instance.
    /// </summary>
    [JsonProperty("customCommands"), JsonPropertyName("customCommands")]
    public InstanceCommandsConfig Commands { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether environment variables are enabled for the instance.
    /// </summary>
    [JsonProperty("enableEnvironment"), JsonPropertyName("enableEnvironment")]
    public bool EnableEnvironment { get; set; }

    /// <summary>
    /// Gets or sets the environment variables for the instance as a dictionary of key-value pairs.
    /// </summary>
    [JsonProperty("environment"), JsonPropertyName("environment")]
    public List<EnvironmentVariable> Environment { get; set; }

    /// <summary>
    /// Gets or sets the miscellaneous configuration for the instance.
    /// </summary>
    [JsonProperty("misc"), JsonPropertyName("misc")]
    public InstanceMiscConfig Misc { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceConfig"/> class with default values.
    /// </summary>
    public InstanceConfig()
    {
        Java = new JavaConfig();
        Game = new InstanceGameConfig();
        Commands = new InstanceCommandsConfig();
        EnableEnvironment = false;
        Environment = [];
        Misc = new InstanceMiscConfig();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InstanceConfig"/> class with the specified parameters.
    /// </summary>
    /// <param name="java">The Java configuration for the game instance.</param>
    /// <param name="game">The game-specific configuration for the instance.</param>
    /// <param name="commands">The custom commands configuration for the instance.</param>
    /// <param name="enableEnvironment">A value indicating whether environment variables are enabled for the instance.</param>
    /// <param name="environment">The list of environment variables for the instance as key-value pairs.</param>
    /// <param name="misc">The miscellaneous configuration for the instance.</param>
    public InstanceConfig(JavaConfig java, InstanceGameConfig game, InstanceCommandsConfig commands, bool enableEnvironment, List<EnvironmentVariable> environment, InstanceMiscConfig misc)
    {
        Java = java;
        Game = game;
        Commands = commands;
        EnableEnvironment = enableEnvironment;
        Environment = environment;
        Misc = misc;
    }
}