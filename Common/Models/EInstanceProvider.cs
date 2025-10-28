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
/// Represents the different instance providers supported by the Konkord Launcher.
/// </summary>
public enum EInstanceProvider
{
    /// <summary>
    /// The Konkord instance provider.
    /// </summary>
    Konkord = 0,

    /// <summary>
    /// The Prism Launcher instance provider.
    /// </summary>
    PrismLauncher = 1,

    /// <summary>
    /// The Modrinth instance provider.
    /// </summary>
    Modrinth = 2,

    /// <summary>
    /// The CurseForge instance provider.
    /// </summary>
    CurseForge = 3
}