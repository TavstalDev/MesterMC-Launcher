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

namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Represents the resolution of a display or window, defined by its width (X) and height (Y).
/// </summary>
[Serializable]
public class Resolution
{
    /// <summary>
    /// Gets or sets the width of the resolution in pixels.
    /// </summary>
    [JsonPropertyName("x"), JsonProperty("x")]
    public uint X { get; set; }

    /// <summary>
    /// Gets or sets the height of the resolution in pixels.
    /// </summary>
    [JsonPropertyName("y"), JsonProperty("y")]
    public uint Y { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Resolution"/> class with default values.
    /// </summary>
    public Resolution() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Resolution"/> class with specified width and height.
    /// </summary>
    /// <param name="x">The width of the resolution in pixels.</param>
    /// <param name="y">The height of the resolution in pixels.</param>
    public Resolution(uint x, uint y)
    {
        X = x;
        Y = y;
    }
}