/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using Tavstal.KonkordLauncher.Core.Models.MojangApi;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for managing and retrieving mod loader manifests.
/// </summary>
public static class ManifestHelper
{
    /// <summary>
    /// Stores the Minecraft version manifest.
    /// </summary>
    private static VersionManifest? _minecraftManifest;

    /// <summary>
    /// Retrieves the cached Minecraft version manifest.
    /// </summary>
    /// <returns>The cached <see cref="VersionManifest"/> or null if not loaded.</returns>
    public static VersionManifest? GetMinecraftManifest() => _minecraftManifest;

    /// <summary>
    /// Asynchronously loads the Minecraft version manifest from the specified path.
    /// </summary>
    /// <param name="manifestPath">The file path to the Minecraft manifest.</param>
    /// <returns>The loaded <see cref="VersionManifest"/> or null if loading fails.</returns>
    public static async Task<VersionManifest?> GetMinecraftManifestAsync(string manifestPath)
    {
        if (_minecraftManifest != null)
            return _minecraftManifest;

        _minecraftManifest = await JsonHelper.ReadJsonFileAsync<VersionManifest>(manifestPath);
        return _minecraftManifest;
    }
}