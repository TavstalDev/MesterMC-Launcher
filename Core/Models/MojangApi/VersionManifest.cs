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

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi;

public class VersionManifest
{
    [JsonPropertyName("latest"), JsonProperty("latest")]
    public VersionManifestLatest Latest {  get; set; }
    [JsonPropertyName("versions"), JsonProperty("versions")]
    public List<MinecraftVersion> Versions { get; set; }

    public VersionManifest()
    {
        Latest = new VersionManifestLatest();
        Versions = [];
    }

    public VersionManifest(VersionManifestLatest latest, List<MinecraftVersion> versions)
    {
        Latest = latest;
        Versions = versions;
    }
}