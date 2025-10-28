/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Enums;

/// <summary>
/// Represents the different platform types supported by the application.
/// </summary>
public enum EPlatformType
{
    /// <summary>
    /// Represents the Modrinth platform.
    /// </summary>
    Modrinth = 0,

    /// <summary>
    /// Represents the CurseForge platform.
    /// </summary>
    CurseForge = 1,

    /// <summary>
    /// Represents the Technic platform.
    /// </summary>
    Technic = 2,

    /// <summary>
    /// Represents the FTB (Feed The Beast) platform.
    /// </summary>
    FTB = 3,
}