/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Helpers;

/// <summary>
/// Provides helper methods for reading and writing JSON files synchronously and asynchronously.
/// </summary>
public static class JsonHelper
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(JsonHelper));

    /// <summary>
    /// Writes an object to a JSON file at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="path">The file path to write the JSON content to.</param>
    /// <param name="obj">The object to serialize into JSON.</param>
    /// <param name="typeInfo"></param>
    /// <returns>True if the operation succeeds, otherwise false.</returns>
    public static bool WriteJsonFile<T>(string path, T obj, JsonTypeInfo<T> typeInfo) =>
        WriteJsonFileAsync(path, obj, typeInfo).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously writes an object to a JSON file at the specified path.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="path">The file path to write the JSON content to.</param>
    /// <param name="obj">The object to serialize into JSON.</param>
    /// <param name="typeInfo"></param>
    /// <returns>True if the operation succeeds, otherwise false.</returns>
    public static async Task<bool> WriteJsonFileAsync<T>(string path, T obj, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            await using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fileStream, obj, typeInfo);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Error in WriteJsonFileAsync<T> {path}:");
            _logger.Error(ex.ToString());
            return false;
        }
    }

    /// <summary>
    /// Reads and deserializes a JSON file into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="path">The file path to read the JSON content from.</param>
    /// <param name="typeInfo"></param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static T? ReadJsonFile<T>(string path, JsonTypeInfo<T> typeInfo) => ReadJsonFileAsync(path, typeInfo).GetAwaiter().GetResult();

    /// <summary>
    /// Asynchronously reads and deserializes a JSON file into an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="path">The file path to read the JSON content from.</param>
    /// <param name="typeInfo">A <see cref="JsonTypeInfo{T}"/> describing the target type for the source-generated serializer.</param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static async Task<T?> ReadJsonFileAsync<T>(string path, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            var local = await JsonSerializer.DeserializeAsync(stream, typeInfo);
            return local;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Error in ReadJsonFileAsync<T> {path}:");
            _logger.Error(ex.ToString());
            return default;
        }
    }
    
    /// <summary>
    /// Asynchronously reads and deserializes an object of type <typeparamref name="T"/> from the provided <see cref="Stream"/>.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize.</typeparam>
    /// <param name="stream">The input stream containing JSON data. The method reads from the stream's current position.</param>
    /// <param name="typeInfo">A <see cref="JsonTypeInfo{T}"/> describing the target type for the source-generated serializer.</param>
    /// <returns>The deserialized object, or default if an error occurs.</returns>
    public static async Task<T?> ReadJsonStreamAsync<T>(Stream stream, JsonTypeInfo<T> typeInfo)
    {
        try
        {
            var local = await JsonSerializer.DeserializeAsync(stream, typeInfo);
            return local;
        }
        catch (Exception ex)
        {
            _logger.Exc($"Error in ReadJsonStreamAsync<T>:");
            _logger.Error(ex.ToString());
            return default;
        }
    }
}