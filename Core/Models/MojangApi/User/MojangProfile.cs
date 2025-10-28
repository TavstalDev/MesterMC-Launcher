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

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

public class MojangProfile
{
    [JsonProperty("id"), JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonProperty("name"), JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonProperty("skins"), JsonPropertyName("skins")]
    public List<Skin> Skins {  get; set; }
    [JsonProperty("capes"), JsonPropertyName("capes")]
    public List<Cape> Capes { get; set; }

    public MojangProfile() { }

    public MojangProfile(string id, string name, List<Skin> skins, List<Cape> capes)
    {
        Id = id;
        Name = name;
        Skins = skins;
        Capes = capes;
    }
}