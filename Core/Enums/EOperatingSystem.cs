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
/// Represents the operating system types supported by the application.
/// </summary>
public enum EOperatingSystem
{
    /// <summary>
    /// Represents the Windows operating system.
    /// </summary>
    Windows = 0,

    /// <summary>
    /// Represents the Linux operating system.
    /// </summary>
    Linux = 1,

    /// <summary>
    /// Represents the macOS operating system.
    /// </summary>
    MacOS = 2,

    /// <summary>
    /// Represents an unknown or unsupported operating system.
    /// </summary>
    Unknown = 3
}