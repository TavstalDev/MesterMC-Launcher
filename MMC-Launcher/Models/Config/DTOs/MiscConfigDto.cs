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
/// Represents miscellaneous configuration settings for the launcher, including custom commands,
/// library paths, and additional runtime options.
/// </summary>
public class MiscConfigDto
{
    /// <summary>
    /// Gets or sets a value indicating whether a custom GLFW library should be used.
    /// </summary>
    [JsonProperty("useCustomGlfw"), JsonPropertyName("useCustomGlfw")]
    public bool UseCustomGlfw { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the custom GLFW library.
    /// </summary>
    [JsonProperty("customGlfwPath"), JsonPropertyName("customGlfwPath")]
    public string CustomGlfwPath { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether a custom OpenAL library should be used.
    /// </summary>
    [JsonProperty("useCustomOpenAl"), JsonPropertyName("useCustomOpenAl")]
    public bool UseCustomOpenAl { get; set; }

    /// <summary>
    /// Gets or sets the file system path to the custom OpenAL library.
    /// </summary>
    [JsonProperty("customOpenAlPath"), JsonPropertyName("customOpenAlPath")]
    public string CustomOpenAlPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Feral GameMode should be enabled.
    /// </summary>
    [JsonProperty("enableFeralGameMode"), JsonPropertyName("enableFeralGameMode")]
    public bool EnableFeralGameMode { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether MangoHud should be enabled.
    /// </summary>
    [JsonProperty("enableMangoHud"), JsonPropertyName("enableMangoHud")]
    public bool EnableMangoHud { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a dedicated GPU should be used.
    /// </summary>
    [JsonProperty("useDedicatedGpu"), JsonPropertyName("useDedicatedGpu")]
    public bool UseDedicatedGpu { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiscConfigDto"/> class with default values.
    /// </summary>
    public MiscConfigDto()
    {
        UseCustomGlfw = false;
        CustomGlfwPath = string.Empty;
        UseCustomOpenAl = false;
        CustomOpenAlPath = string.Empty;
        EnableFeralGameMode = false;
        EnableMangoHud = false;
        UseDedicatedGpu = true;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiscConfigDto"/> class with specified values.
    /// </summary>
    /// <param name="useCustomGlfw">Whether a custom GLFW library should be used.</param>
    /// <param name="customGlfwPath">The file system path to the custom GLFW library.</param>
    /// <param name="useCustomOpenAl">Whether a custom OpenAL library should be used.</param>
    /// <param name="customOpenAlPath">The file system path to the custom OpenAL library.</param>
    /// <param name="enableFeralGameMode">Whether Feral GameMode should be enabled.</param>
    /// <param name="enableMangoHud">Whether MangoHud should be enabled.</param>
    /// <param name="useDedicatedGpu">Whether a dedicated GPU should be used.</param>
    public MiscConfigDto(bool useCustomGlfw, string customGlfwPath, bool useCustomOpenAl, string customOpenAlPath, bool enableFeralGameMode, bool enableMangoHud, bool useDedicatedGpu)
    {
        UseCustomGlfw = useCustomGlfw;
        CustomGlfwPath = customGlfwPath;
        UseCustomOpenAl = useCustomOpenAl;
        CustomOpenAlPath = customOpenAlPath;
        EnableFeralGameMode = enableFeralGameMode;
        EnableMangoHud = enableMangoHud;
        UseDedicatedGpu = useDedicatedGpu;
    }
}