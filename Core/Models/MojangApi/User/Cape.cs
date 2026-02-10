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

public class Cape
{
    [JsonPropertyName("id"), JsonProperty("id")]
    public string Id {  get; set; }
    [JsonPropertyName("state"), JsonProperty("state")]
    public string State {  get; set; }
    [JsonPropertyName("url"), JsonProperty("url")]
    public string Url {  get; set; }
    [JsonPropertyName("alias"), JsonProperty("alias")]
    public string Alias {  get; set; }

    public Cape() { }

    public Cape(string id, string state, string url, string alias)
    {
        Id = id;
        State = state;
        Url = url;
        Alias = alias;
    }
}