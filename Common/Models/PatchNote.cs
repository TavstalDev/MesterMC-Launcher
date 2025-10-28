/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents a patch note with a title, content, and a URL for more details.
/// </summary>
public class PatchNote
{
    /// <summary>
    /// Gets or sets the title of the patch note.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the content or description of the patch note.
    /// </summary>
    public string Content { get; set; }

    /// <summary>
    /// Gets or sets the URL for more information about the patch note.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PatchNote"/> class with the specified title, content, and URL.
    /// </summary>
    /// <param name="title">The title of the patch note.</param>
    /// <param name="content">The content or description of the patch note.</param>
    /// <param name="url">The URL for more information about the patch note.</param>
    public PatchNote(string title, string content, string url)
    {
        Title = title;
        Content = content;
        Url = url;
    }
}