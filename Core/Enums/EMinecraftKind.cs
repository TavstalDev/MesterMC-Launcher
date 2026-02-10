/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Enums;

/// <summary>
/// Represents the different kinds of Minecraft profiles supported by the launcher.
/// </summary>
public enum EMinecraftKind
{
    /// <summary>
    /// Represents the Vanilla version of Minecraft.
    /// </summary>
    VANILLA = 0,

    /// <summary>
    /// Represents the NeoForge modded version of Minecraft.
    /// </summary>
    NEOFORGE = 1,
    
    /// <summary>
    /// Represents the Forge modded version of Minecraft.
    /// </summary>
    FORGE = 2,

    /// <summary>
    /// Represents the Fabric modded version of Minecraft.
    /// </summary>
    FABRIC = 3,

    /// <summary>
    /// Represents the Quilt modded version of Minecraft.
    /// </summary>
    QUILT = 4
}