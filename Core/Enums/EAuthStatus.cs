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
/// Represents the status of an authentication process.
/// </summary>
public enum EAuthStatus
{
    /// <summary>
    /// Represents an undefined or uninitialized authentication status.
    /// </summary>
    NONE = 0,
    
    /// <summary>
    /// The authentication process is pending.
    /// </summary>
    PENDING = 1,

    /// <summary>
    /// The authentication process was successful.
    /// </summary>
    SUCCESS = 2,

    /// <summary>
    /// The authentication process failed.
    /// </summary>
    FAILED = 3,
}