/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Models.Installer;

/// <summary>
/// Represents a launch argument with a specified priority.
/// </summary>
public class LaunchArg
{
    /// <summary>
    /// Gets or sets the launch argument as a string.
    /// </summary>
    public string Arg { get; set; }

    /// <summary>
    /// Gets or sets the priority of the launch argument.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LaunchArg"/> class.
    /// </summary>
    public LaunchArg() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LaunchArg"/> class with a specified argument and priority.
    /// </summary>
    /// <param name="arg">The launch argument as a string.</param>
    /// <param name="priority">The priority of the launch argument.</param>
    public LaunchArg(string arg, int priority)
    {
        Arg = arg;
        Priority = priority;
    }
}