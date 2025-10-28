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
/// Represents the type of profile used in the application.
/// </summary>
public enum EProfileType
{
    /// <summary>
    /// Represents the latest release version of the profile.
    /// </summary>
    LATEST_RELEASE = 0,

    /// <summary>
    /// Represents the latest snapshot version of the profile.
    /// </summary>
    LATEST_SNAPSHOT = 1,

    /// <summary>
    /// Represents a custom profile type.
    /// </summary>
    CUSTOM = 2
}