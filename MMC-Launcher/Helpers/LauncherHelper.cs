/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.MesterMC.Launcher.Models;
using Tavstal.MesterMC.Launcher.Models.Config.DTOs;
using Tavstal.MesterMC.Launcher.Models.Json;

namespace Tavstal.MesterMC.Launcher.Helpers;

/// <summary>
/// Provides helper methods for managing launcher settings, accounts, and instances.
/// </summary>
public static class LauncherHelper
{
    /// <summary>
    /// Retrieves the launcher settings from the configuration file.
    /// If the file does not exist or is invalid, a new configuration is created and saved.
    /// </summary>
    /// <returns>The launcher settings as a <see cref="CoreConfigDto"/> object.</returns>
    public static CoreConfigDto GetLauncherSettings()
    {
        if (!File.Exists(PathHelper.LauncherConfigPath))
        {
            CoreConfigDto result = new CoreConfigDto();
            JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, result, CustomJsonContext.Default.CoreConfigDto);
            return result;
        }

        var readResult = JsonHelper.ReadJsonFile<CoreConfigDto>(PathHelper.LauncherConfigPath, CustomJsonContext.Default.CoreConfigDto);
        if (readResult == null)
        {
            CoreConfigDto result = new CoreConfigDto();
            File.Move(PathHelper.LauncherConfigPath, PathHelper.LauncherConfigPath + ".bak", true);
            JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, result, CustomJsonContext.Default.CoreConfigDto);
            return result;
        }

        return readResult;
    }

    /// <summary>
    /// Asynchronously retrieves the launcher settings from the configuration file.
    /// If the file does not exist or is invalid, a new configuration is created and saved.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the launcher settings as a <see cref="CoreConfigDto"/> object.</returns>
    public static async Task<CoreConfigDto> GetLauncherSettingsAsync()
    {
        if (!File.Exists(PathHelper.LauncherConfigPath))
        {
            CoreConfigDto result = new CoreConfigDto();
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, result, CustomJsonContext.Default.CoreConfigDto);
            return result;
        }

        var readResult = await JsonHelper.ReadJsonFileAsync<CoreConfigDto>(PathHelper.LauncherConfigPath, CustomJsonContext.Default.CoreConfigDto);
        if (readResult == null)
        {
            CoreConfigDto result = new CoreConfigDto();
            File.Move(PathHelper.LauncherConfigPath, PathHelper.LauncherConfigPath + ".bak", true);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, result, CustomJsonContext.Default.CoreConfigDto);
            return result;
        }

        return readResult;
    }
    
    /// <summary>
    /// Retrieves the news data from the specified cache directory.
    /// If the file does not exist or is invalid, a new empty list is created and saved.
    /// </summary>
    /// <param name="cacheDir">The directory where the news data is cached.</param>
    /// <returns>A list of <see cref="NewsDto"/> objects representing the news data.</returns>
    public static List<NewsDto> GetNews(string cacheDir)
    {
        string newsPath = Path.Combine(cacheDir, "news.json");
        if (!File.Exists(newsPath))
        {
            List<NewsDto> result = new();
            JsonHelper.WriteJsonFile(newsPath, result, CustomJsonContext.Default.ListNewsDto);
            return result;
        }

        var readResult = JsonHelper.ReadJsonFile<List<NewsDto>>(newsPath, CustomJsonContext.Default.ListNewsDto);
        if (readResult == null)
        {
            List<NewsDto> result = new();
            File.Move(newsPath, newsPath + ".bak", true);
            JsonHelper.WriteJsonFile(newsPath, result, CustomJsonContext.Default.ListNewsDto);
            return result;
        }

        return readResult;
    }
    
    /// <summary>
    /// Asynchronously retrieves the news data from the specified cache directory.
    /// If the file does not exist or is invalid, a new empty list is created and saved.
    /// </summary>
    /// <param name="cacheDir">The directory where the news data is cached.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a list of 
    /// <see cref="NewsDto"/> objects representing the news data.
    /// </returns>
    public static async Task<List<NewsDto>> GetNewsAsync(string cacheDir)
    {
        string newsPath = Path.Combine(cacheDir, "news.json");
        if (!File.Exists(newsPath))
        {
            List<NewsDto> result = new();
            await JsonHelper.WriteJsonFileAsync(newsPath, result, CustomJsonContext.Default.ListNewsDto);
            return result;
        }

        var readResult = await JsonHelper.ReadJsonFileAsync<List<NewsDto>>(newsPath, CustomJsonContext.Default.ListNewsDto);
        if (readResult == null)
        {
            List<NewsDto> result = new();
            File.Move(newsPath, newsPath + ".bak", true);
            await JsonHelper.WriteJsonFileAsync(newsPath, result, CustomJsonContext.Default.ListNewsDto);
            return result;
        }

        return readResult;
    }
}