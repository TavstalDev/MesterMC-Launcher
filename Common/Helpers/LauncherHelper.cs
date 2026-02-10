/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Common.Models.Config;
using Tavstal.KonkordLauncher.Common.Models.Json;
using Tavstal.KonkordLauncher.Core.Helpers;

namespace Tavstal.KonkordLauncher.Common.Helpers;

/// <summary>
/// Provides helper methods for managing launcher settings, accounts, and instances.
/// </summary>
public static class LauncherHelper
{
    /// <summary>
    /// Retrieves the launcher settings from the configuration file.
    /// If the file does not exist or is invalid, a new configuration is created and saved.
    /// </summary>
    /// <returns>The launcher settings as a <see cref="CoreConfig"/> object.</returns>
    public static CoreConfig GetLauncherSettings()
    {
        if (!File.Exists(PathHelper.LauncherConfigPath))
        {
            CoreConfig result = new CoreConfig();
            JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, result, CommonJsonContext.Default.CoreConfig);
            return result;
        }

        var readResult = JsonHelper.ReadJsonFile<CoreConfig>(PathHelper.LauncherConfigPath, CommonJsonContext.Default.CoreConfig);
        if (readResult == null)
        {
            CoreConfig result = new CoreConfig();
            File.Move(PathHelper.LauncherConfigPath, PathHelper.LauncherConfigPath + ".bak", true);
            JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, result, CommonJsonContext.Default.CoreConfig);
            return result;
        }

        return readResult;
    }

    /// <summary>
    /// Asynchronously retrieves the launcher settings from the configuration file.
    /// If the file does not exist or is invalid, a new configuration is created and saved.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the launcher settings as a <see cref="CoreConfig"/> object.</returns>
    public static async Task<CoreConfig> GetLauncherSettingsAsync()
    {
        if (!File.Exists(PathHelper.LauncherConfigPath))
        {
            CoreConfig result = new CoreConfig();
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, result, CommonJsonContext.Default.CoreConfig);
            return result;
        }

        var readResult = await JsonHelper.ReadJsonFileAsync<CoreConfig>(PathHelper.LauncherConfigPath, CommonJsonContext.Default.CoreConfig);
        if (readResult == null)
        {
            CoreConfig result = new CoreConfig();
            File.Move(PathHelper.LauncherConfigPath, PathHelper.LauncherConfigPath + ".bak", true);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, result, CommonJsonContext.Default.CoreConfig);
            return result;
        }

        return readResult;
    }
    
    /// <summary>
    /// Retrieves the news data from the specified cache directory.
    /// If the file does not exist or is invalid, a new empty list is created and saved.
    /// </summary>
    /// <param name="cacheDir">The directory where the news data is cached.</param>
    /// <returns>A list of <see cref="NewsData"/> objects representing the news data.</returns>
    public static List<NewsData> GetNews(string cacheDir)
    {
        string newsPath = Path.Combine(cacheDir, "news.json");
        if (!File.Exists(newsPath))
        {
            List<NewsData> result = new();
            JsonHelper.WriteJsonFile(newsPath, result, CommonJsonContext.Default.ListNewsData);
            return result;
        }

        var readResult = JsonHelper.ReadJsonFile<List<NewsData>>(newsPath, CommonJsonContext.Default.ListNewsData);
        if (readResult == null)
        {
            List<NewsData> result = new();
            File.Move(newsPath, newsPath + ".bak", true);
            JsonHelper.WriteJsonFile(newsPath, result, CommonJsonContext.Default.ListNewsData);
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
    /// <see cref="NewsData"/> objects representing the news data.
    /// </returns>
    public static async Task<List<NewsData>> GetNewsAsync(string cacheDir)
    {
        string newsPath = Path.Combine(cacheDir, "news.json");
        if (!File.Exists(newsPath))
        {
            List<NewsData> result = new();
            await JsonHelper.WriteJsonFileAsync(newsPath, result, CommonJsonContext.Default.ListNewsData);
            return result;
        }

        var readResult = await JsonHelper.ReadJsonFileAsync<List<NewsData>>(newsPath, CommonJsonContext.Default.ListNewsData);
        if (readResult == null)
        {
            List<NewsData> result = new();
            File.Move(newsPath, newsPath + ".bak", true);
            await JsonHelper.WriteJsonFileAsync(newsPath, result, CommonJsonContext.Default.ListNewsData);
            return result;
        }

        return readResult;
    }
}