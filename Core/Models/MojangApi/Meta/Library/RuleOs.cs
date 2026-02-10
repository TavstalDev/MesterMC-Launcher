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

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

/// <summary>
/// Represents the operating system specification for a rule.
/// </summary>
public class RuleOs
{
    /// <summary>
    /// Gets or sets the name of the operating system.
    /// </summary>
    [JsonPropertyName("name"), JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleOs"/> class.
    /// </summary>
    public RuleOs() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuleOs"/> class with a specified operating system name.
    /// </summary>
    /// <param name="name">The name of the operating system.</param>
    public RuleOs(string name)
    {
        Name = name;
    }
}