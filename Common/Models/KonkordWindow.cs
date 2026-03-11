/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */


using Avalonia.Controls;
using ReactiveUI.Avalonia;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents a generic window that integrates with ReactiveUI and supports a strongly-typed ViewModel.
/// Automatically disposes of the ViewModel if it implements IDisposable when the window is closing.
/// </summary>
/// <typeparam name="TViewModel">The type of the ViewModel associated with this window.</typeparam>
public abstract class KonkordWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    /// <summary>
    /// Gets or sets the strongly-typed DataContext for the window.
    /// Overrides the base DataContext property to enforce the type constraint.
    /// </summary>
    public new TViewModel? DataContext
    {
        get => (TViewModel?)base.DataContext;
        set => base.DataContext = value;
    }
    /// <summary>
    /// Called when the window is closing.
    /// Disposes of the DataContext if it implements IDisposable, then calls the base implementation.
    /// </summary>
    /// <param name="e">The event arguments for the window closing event.</param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosing(e);
    }
}