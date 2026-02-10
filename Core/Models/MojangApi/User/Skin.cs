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

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.User;

public class Skin
{
    [JsonProperty("id"), JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonProperty("state"), JsonPropertyName("state")]
    public string State { get; set; }
    [JsonProperty("url"), JsonPropertyName("url")]
    public string Url { get; set; }
    [JsonProperty("variant"), JsonPropertyName("variant")]
    public string Variant { get; set; }
    [JsonProperty("alias"), JsonPropertyName("alias")]
    public string? Alias { get; set; }

    public Skin() { }
    public Skin(string id, string state, string url, string variant, string? alias)
    {
        Id = id;
        State = state;
        Url = url;
        Variant = variant;
        Alias = alias;
    }
}