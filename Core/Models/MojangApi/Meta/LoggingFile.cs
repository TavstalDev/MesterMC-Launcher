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

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

/// <summary>
/// Represents a logging file with metadata such as ID, SHA-1 hash, URL, and size.
/// </summary>
public class LoggingFile
{
    /// <summary>
    /// Gets or sets the SHA-1 hash of the logging file.
    /// </summary>
    [JsonPropertyName("sha1"), JsonProperty("sha1")]
    public string Sha1 { get; set; }

    /// <summary>
    /// Gets or sets the URL of the logging file.
    /// </summary>
    [JsonPropertyName("url"), JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the size of the logging file in bytes.
    /// </summary>
    [JsonPropertyName("size"), JsonProperty("size")]
    public int Size { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the logging file.
    /// </summary>
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingFile"/> class.
    /// </summary>
    public LoggingFile() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingFile"/> class with specified properties.
    /// </summary>
    /// <param name="id">The unique identifier of the logging file.</param>
    /// <param name="sha1">The SHA-1 hash of the logging file.</param>
    /// <param name="url">The URL of the logging file.</param>
    /// <param name="size">The size of the logging file in bytes.</param>
    public LoggingFile(string id, string sha1, string url, int size)
    {
        Id = id;
        Sha1 = sha1;
        Url = url;
        Size = size;
    }
}