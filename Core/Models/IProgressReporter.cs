/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

namespace Tavstal.KonkordLauncher.Core.Models;

/// <summary>
/// Represents a progress reporter interface for tracking and displaying progress and status updates.
/// </summary>
public interface IProgressReporter
{
    /// <summary>
    /// Sets the progress value.
    /// </summary>
    /// <param name="progress">The progress value as a double, typically between 0.0 and 1.0.</param>
    void SetProgress(double progress);

    /// <summary>
    /// Sets the status message.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    void SetStatus(string status);
    
    /// <summary>
    /// Sets the status message using a formatted string and optional arguments.
    /// </summary>
    /// <param name="status">The status message format string.</param>
    /// <param name="args">Optional arguments to format the status message.</param>
    void SetStatus(string status, params object[]? args);

    /// <summary>
    /// Displays the progress reporter.
    /// </summary>
    void Show();

    /// <summary>
    /// Hides the progress reporter.
    /// </summary>
    void Hide();
}