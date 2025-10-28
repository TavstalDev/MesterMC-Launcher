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

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

/// <summary>
/// Represents a rule with an action and an optional operating system specification.
/// </summary>
public class Rule
{
    /// <summary>
    /// Gets or sets the action of the rule (e.g., allow or disallow).
    /// </summary>
    [JsonPropertyName("action"), JsonProperty("action")]
    public string Action { get; set; }

    /// <summary>
    /// Gets or sets the operating system specification for the rule, if applicable.
    /// </summary>
    [JsonPropertyName("OS"), JsonProperty("OS")]
    public RuleOs Os { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rule"/> class.
    /// </summary>
    public Rule() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Rule"/> class with specified action and operating system.
    /// </summary>
    /// <param name="action">The action of the rule.</param>
    /// <param name="oS">The operating system specification for the rule.</param>
    public Rule(string action, RuleOs oS)
    {
        Action = action;
        Os = oS;
    }
}