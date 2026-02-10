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
/// Represents the logging client configuration, including arguments, file metadata, and type.
/// </summary>
public class LoggingClient
{
    /// <summary>
    /// Gets or sets the argument used for the logging client configuration.
    /// </summary>
    [JsonPropertyName("argument"), JsonProperty("argument")]
    public string Argument { get; set; }

    /// <summary>
    /// Gets or sets the logging file metadata associated with the logging client.
    /// </summary>
    [JsonPropertyName("file"), JsonProperty("file")]
    public LoggingFile File { get; set; }

    /// <summary>
    /// Gets or sets the type of the logging client configuration.
    /// </summary>
    [JsonPropertyName("type"), JsonProperty("type")]
    public string Type { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingClient"/> class.
    /// </summary>
    public LoggingClient() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingClient"/> class with specified properties.
    /// </summary>
    /// <param name="argument">The argument used for the logging client configuration.</param>
    /// <param name="file">The logging file metadata associated with the logging client.</param>
    /// <param name="type">The type of the logging client configuration.</param>
    public LoggingClient(string argument, LoggingFile file, string type)
    {
        Argument = argument;
        File = file;
        Type = type;
    }
}