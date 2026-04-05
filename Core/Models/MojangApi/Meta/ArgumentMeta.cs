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
using Newtonsoft.Json.Linq;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;

// ReSharper disable ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract - The properties in this class can be null based on the JSON structure, so we disable the warning for conditions that may always be true or false according to nullable reference types.

/// <summary>
/// Represents metadata for game and JVM arguments used in Minecraft.
/// </summary>
public class ArgumentMeta
{
    /// <summary>
    /// Gets or sets the list of game arguments.
    /// </summary>
    [JsonPropertyName("game"), JsonProperty("game")]
    public List<object> Game { get; set; }

    /// <summary>
    /// Gets or sets the list of JVM arguments.
    /// </summary>
    [JsonPropertyName("jvm"), JsonProperty("jvm")]
    public List<object> Jvm { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentMeta"/> class.
    /// </summary>
    public ArgumentMeta() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArgumentMeta"/> class with specified game and JVM arguments.
    /// </summary>
    /// <param name="game">The list of game arguments.</param>
    /// <param name="jvm">The list of JVM arguments.</param>
    public ArgumentMeta(List<object> game, List<object> jvm)
    {
        Game = game;
        Jvm = jvm;
    }

    /// <summary>
    /// Retrieves the game arguments as a list of strings.
    /// </summary>
    /// <returns>A list of game arguments.</returns>
    public List<string> GetGameArgs()
    {
        if (Game == null)
            return [];

        List<string> local = [];
        foreach (var item in Game)
        {
            if (item is string s)
                local.Add(s);
        }

        List<string> result = [];

        for (int i = 0; i < local.Count; i += 2)
        {
            result.Add(local[i] + " " + local[i + 1]);
        }

        return result;
    }

    /// <summary>
    /// Retrieves the game arguments as a single concatenated string.
    /// </summary>
    /// <returns>A string containing the game arguments.</returns>
    public string GetGameArgString()
    {
        if (Game == null)
            return string.Empty;

        List<string> local = [];
        foreach (var item in Game)
        {
            if (item is string s)
                local.Add(s);
        }

        string result = string.Empty;
        for (int i = 0; i < local.Count; i += 2)
        {
            result += $"{local[i]} {local[i + 1]} ";
        }

        return result;
    }

    /// <summary>
    /// Retrieves the JVM arguments as a list of strings.
    /// </summary>
    /// <returns>A list of JVM arguments.</returns>
    public List<string> GetJvmArgs()
    {
        if (Jvm == null)
            return [];

        List<string> local = [];
        foreach (var item in Jvm)
        {
            if (item is string s)
                local.Add(s);
            else
            {
                var raw = item.ToString();
                if (raw == null)
                    continue;
               
                var ruleResult = GetRuleResult(raw);
                if (string.IsNullOrEmpty(ruleResult))
                    continue;
                
                local.Add(ruleResult);
            }
        }

        return local;
    }

    /// <summary>
    /// Retrieves the JVM arguments as a single concatenated string.
    /// </summary>
    /// <returns>A string containing the JVM arguments.</returns>
    public string GetJvmArgString()
    {
        if (Jvm == null)
            return string.Empty;

        string local = string.Empty;
        foreach (var item in Jvm)
        {
            if (item is string s)
            {
                local += $"{s} ";
            }
            else
            {
                var raw = item.ToString();
                if (raw == null)
                    continue;
               
                var ruleResult = GetRuleResult(raw);
                if (string.IsNullOrEmpty(ruleResult))
                    continue;
                
                local += $"{ruleResult} ";
            }
        }

        return local;
    }

    /// <summary>
    /// Evaluates a set of rules from a JSON string and determines whether the rules allow or deny access.
    /// </summary>
    /// <param name="json">The JSON string containing the rules and value to evaluate.</param>
    /// <returns>
    /// The value from the JSON if the rules allow access; otherwise, <c>null</c>.
    /// </returns>
    public string? GetRuleResult(string json)
    {
        JObject jobj = JObject.Parse(json);
        string value = jobj["value"]?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(value))
            return null;
                
        var rawRules = jobj["rules"]?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(rawRules))
            return null;
        List<Rule>? rules =  JsonConvert.DeserializeObject<List<Rule>>(rawRules);
        if (rules == null || rules.Count == 0)
            return null;
                
        bool localResult = false;
        var operatingSystem = OSHelper.GetOperatingSystem();
        foreach (Rule rule in rules)
        {
            if (rule.Os == null)
            {
                localResult = rule.Action == "allow";
                continue;
            }
            
            if (rule.Os.Name == null)
            {
                localResult = rule.Action == "allow";
                continue;
            }

            if (rule.Os.Name == "x86" && !OSHelper.Is64BitOperatingSystem())
            {
                localResult = rule.Action == "allow";
                continue;
            }
                
            if (rule.Os.Name == "windows" && operatingSystem == EOperatingSystem.Windows)
            {
                localResult = rule.Action == "allow";
                continue;
            }

            if (rule.Os.Name == "linux" && operatingSystem == EOperatingSystem.Linux)
            {
                localResult = rule.Action == "allow";
                continue;
            }

            if (rule.Os.Name.StartsWith("osx") && operatingSystem == EOperatingSystem.MacOS)
                localResult = rule.Action == "allow";
        }
        
        return localResult ? value : null;
    }
}