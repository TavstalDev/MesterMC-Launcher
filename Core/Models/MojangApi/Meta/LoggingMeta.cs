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

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents metadata for logging configuration in Minecraft.
/// </summary>
public class LoggingMeta
{
    /// <summary>
    /// Gets or sets the logging client configuration.
    /// </summary>
    [JsonPropertyName("client"), JsonProperty("client")]
    public LoggingClient Client { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingMeta"/> class.
    /// </summary>
    public LoggingMeta()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingMeta"/> class with a specified logging client configuration.
    /// </summary>
    /// <param name="client">The logging client configuration.</param>
    public LoggingMeta(LoggingClient client)
    {
        Client = client;
    }
}