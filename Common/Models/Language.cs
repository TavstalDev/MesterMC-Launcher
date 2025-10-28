/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Globalization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents a language with its associated metadata and functionality.
/// </summary>
public class Language
{
    /// <summary>
    /// Gets or sets the name of the language.
    /// </summary>
    [JsonProperty("name"), JsonPropertyName("name")]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the two-letter ISO 639-1 code for the language.
    /// </summary>
    [JsonProperty("twoLetterCode"), JsonPropertyName("twoLetterCode")]
    public string TwoLetterCode { get; set; }

    /// <summary>
    /// Gets or sets the URL for the language's translation file.
    /// </summary>
    [JsonProperty("url"), JsonPropertyName("url")]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this language is the default language.
    /// </summary>
    [JsonProperty("isDefault"), JsonPropertyName("isDefault")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class.
    /// </summary>
    public Language() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class with the specified name, codes, and URL.
    /// </summary>
    /// <param name="name">The name of the language.</param>
    /// <param name="twoLetterCode">The two-letter ISO 639-1 code for the language.</param>
    /// <param name="url">The URL for the language's translation file.</param>
    public Language(string name, string twoLetterCode, string url)
    {
        Name = name;
        TwoLetterCode = twoLetterCode;
        Url = url;
        IsDefault = false;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Language"/> class with the specified name, codes, URL, and default status.
    /// </summary>
    /// <param name="name">The name of the language.</param>
    /// <param name="twoLetterCode">The two-letter ISO 639-1 code for the language.</param>
    /// <param name="url">The URL for the language's translation file.</param>
    /// <param name="isDefault">A value indicating whether this language is the default language.</param>
    public Language(string name, string twoLetterCode, string url, bool isDefault)
    {
        Name = name;
        TwoLetterCode = twoLetterCode;
        Url = url;
        IsDefault = isDefault;
    }

    /// <summary>
    /// Gets the <see cref="CultureInfo"/> object associated with the language's two-letter code.
    /// </summary>
    /// <returns>A <see cref="CultureInfo"/> object for the language.</returns>
    public CultureInfo GetCultureInfo()
    {
        return new CultureInfo(TwoLetterCode);
    }
}