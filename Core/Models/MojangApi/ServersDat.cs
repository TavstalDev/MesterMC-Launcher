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
using NbtLib;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

/// <summary>
/// Represents the data structure for storing a list of Minecraft servers.
/// </summary>
public class ServersDat
{
    /// <summary>
    /// Gets or sets the list of Minecraft servers.
    /// </summary>
    [NbtProperty(PropertyName="servers")]
    [JsonProperty("servers"), JsonPropertyName("servers")]
    public List<MinecraftServer> Servers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServersDat"/> class
    /// with an empty list of servers.
    /// </summary>
    public ServersDat()
    {
        Servers = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServersDat"/> class
    /// with the specified list of servers.
    /// </summary>
    /// <param name="servers">The list of Minecraft servers.</param>
    public ServersDat(List<MinecraftServer> servers)
    {
        Servers = servers;
    }
}