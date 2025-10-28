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
/// Represents an environment variable with a key-value pair.
/// </summary>
public class EnvironmentVariable
{
    /// <summary>
    /// Gets or sets the key of the environment variable.
    /// </summary>
    [JsonProperty("key"), JsonPropertyName("key")]
    public string Key { get; set; }

    /// <summary>
    /// Gets or sets the value of the environment variable.
    /// </summary>
    [JsonProperty("value"), JsonPropertyName("value")]
    public string Value { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentVariable"/> class
    /// with the specified key and value.
    /// </summary>
    /// <param name="key">The key of the environment variable.</param>
    /// <param name="value">The value of the environment variable.</param>
    public EnvironmentVariable(string key, string value)
    {
        Key = key;
        Value = value;
    }
}