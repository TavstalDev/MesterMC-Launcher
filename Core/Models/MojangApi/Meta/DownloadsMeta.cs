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
/// Represents metadata for various Minecraft downloads, including client, server, and their mappings.
/// </summary>
public class DownloadsMeta
{
    /// <summary>
    /// Gets or sets the download data for the Minecraft client.
    /// </summary>
    [JsonPropertyName("client"), JsonProperty("client")]
    public DownloadData Client { get; set; }

    /// <summary>
    /// Gets or sets the download data for the Minecraft client mappings.
    /// </summary>
    [JsonPropertyName("client_mappings"), JsonProperty("client_mappings")]
    public DownloadData ClientMappings { get; set; }

    /// <summary>
    /// Gets or sets the download data for the Minecraft server.
    /// </summary>
    [JsonPropertyName("server"), JsonProperty("server")]
    public DownloadData Server { get; set; }

    /// <summary>
    /// Gets or sets the download data for the Minecraft server mappings.
    /// </summary>
    [JsonPropertyName("server_mappings"), JsonProperty("server_mappings")]
    public DownloadData ServerMappings { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadsMeta"/> class.
    /// </summary>
    public DownloadsMeta()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadsMeta"/> class with specified download data.
    /// </summary>
    /// <param name="client">The download data for the Minecraft client.</param>
    /// <param name="clientMappings">The download data for the Minecraft client mappings.</param>
    /// <param name="server">The download data for the Minecraft server.</param>
    /// <param name="serverMappings">The download data for the Minecraft server mappings.</param>
    public DownloadsMeta(DownloadData client, DownloadData clientMappings, DownloadData server,
        DownloadData serverMappings)
    {
        Client = client;
        ClientMappings = clientMappings;
        Server = server;
        ServerMappings = serverMappings;
    }
}