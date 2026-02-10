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

namespace Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

/// <summary>
/// Represents the downloads for a library, including the main artifact and optional classifiers for platform-specific artifacts.
/// </summary>
public class LibraryDownloads
{
    /// <summary>
    /// Gets or sets the main artifact associated with the library.
    /// </summary>
    [JsonPropertyName("artifact"), JsonProperty("artifact")]
    public Artifact? Artifact { get; set; }

    /// <summary>
    /// Gets or sets the classifiers for platform-specific artifacts, if available.
    /// </summary>
    [JsonPropertyName("classifiers"), JsonProperty("classifiers")]
    public Classifier? Classifiers { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryDownloads"/> class.
    /// </summary>
    public LibraryDownloads()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryDownloads"/> class with specified artifact and classifiers.
    /// </summary>
    /// <param name="artifact">The main artifact associated with the library.</param>
    /// <param name="classifiers">The classifiers for platform-specific artifacts, if available.</param>
    public LibraryDownloads(Artifact artifact, Classifier? classifiers)
    {
        Artifact = artifact;
        Classifiers = classifiers;
    }
}