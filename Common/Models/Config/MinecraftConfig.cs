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
/// Represents the configuration settings for Minecraft, including window properties
/// and launcher behavior during game start and exit.
/// </summary>
public class MinecraftConfig
{
    /// <summary>
    /// Gets or sets a value indicating whether the game should start maximized.
    /// </summary>
    [JsonProperty("startMaximized"), JsonPropertyName("startMaximized")]
    public bool StartMaximized { get; set; }
    
    /// <summary>
    /// Gets or sets the width of the game window in pixels.
    /// </summary>
    [JsonProperty("windowWidth"), JsonPropertyName("windowWidth")]
    public uint WindowWidth { get; set; }
    
    /// <summary>
    /// Gets or sets the height of the game window in pixels.
    /// </summary>
    [JsonProperty("windowHeight"), JsonPropertyName("windowHeight")]
    public uint WindowHeight { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftConfig"/> class with default values.
    /// </summary>
    public MinecraftConfig()
    {
        StartMaximized = false;
        WindowWidth = 1280;
        WindowHeight = 720;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftConfig"/> class with specified values.
    /// </summary>
    /// <param name="startMaximized">Whether the game should start maximized.</param>
    /// <param name="windowWidth">The width of the game window in pixels.</param>
    /// <param name="windowHeight">The height of the game window in pixels.</param>
    public MinecraftConfig(bool startMaximized, uint windowWidth, uint windowHeight)
    {
        StartMaximized = startMaximized;
        WindowWidth = windowWidth;
        WindowHeight = windowHeight;
    }
}