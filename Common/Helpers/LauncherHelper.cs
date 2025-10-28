/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using Tavstal.KonkordLauncher.Common.Models.Config;
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
            JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, result);
            return result;
        }

        var readResult = JsonHelper.ReadJsonFile<CoreConfig>(PathHelper.LauncherConfigPath);
        if (readResult == null)
        {
            CoreConfig result = new CoreConfig();
            File.Move(PathHelper.LauncherConfigPath, PathHelper.LauncherConfigPath + ".bak", true);
            JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, result);
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
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, result);
            return result;
        }

        var readResult = await JsonHelper.ReadJsonFileAsync<CoreConfig>(PathHelper.LauncherConfigPath);
        if (readResult == null)
        {
            CoreConfig result = new CoreConfig();
            File.Move(PathHelper.LauncherConfigPath, PathHelper.LauncherConfigPath + ".bak", true);
            await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, result);
            return result;
        }

        return readResult;
    }
}