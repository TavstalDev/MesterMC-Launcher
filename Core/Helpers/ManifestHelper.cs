/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Text.Json.Nodes;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Fabric;
using Tavstal.KonkordLauncher.Core.Models.Json;
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

        _minecraftManifest = await JsonHelper.ReadJsonFileAsync<VersionManifest>(manifestPath, CoreJsonContext.Default.VersionManifest);
        return _minecraftManifest;
    }
    
    /// <summary>
    /// Stores the Fabric mod loader manifests.
    /// </summary>
    private static List<IModManifest>? _fabricManifest;

    /// <summary>
    /// Retrieves the cached Fabric mod loader manifests.
    /// </summary>
    /// <returns>A list of <see cref="IModManifest"/> or null if not loaded.</returns>
    public static List<IModManifest>? GetFabricManifest() => _fabricManifest;

    /// <summary>
    /// Asynchronously loads the Fabric mod loader manifests from the specified path.
    /// </summary>
    /// <param name="manifestPath">The file path to the Fabric manifest.</param>
    /// <returns>A list of <see cref="IModManifest"/> or null if loading fails.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the loader section is missing in the JSON.</exception>
    public static async Task<List<IModManifest>?> GetFabricManifestAsync(string manifestPath)
    {
        if (_fabricManifest != null)
            return _fabricManifest;

        var rawManifest = await File.ReadAllTextAsync(manifestPath);
        JsonObject? jObject = JsonNode.Parse(rawManifest)?.AsObject();
        if (jObject == null)
            throw new Exception("Unable to deserialize the manifest file");
        var mappings = jObject["loader"]?.AsArray();
        if (mappings == null)
        {
            throw new InvalidOperationException("Fabric manifest loader not found in the JSON.");
        }
        _fabricManifest = [];
        foreach (var mapping in mappings)
        {
            if (mapping == null)
                continue;
            
            var result = mapping["version"]?.GetValue<string>();
            if (string.IsNullOrEmpty(result))
                continue;
            _fabricManifest.Add(new FabricManifest(result));
        }

        return _fabricManifest;
    }
}