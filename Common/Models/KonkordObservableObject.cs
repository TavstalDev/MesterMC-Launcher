/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */


using System.Reactive.Disposables;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.KonkordLauncher.Common.Models;

/// <summary>
/// Represents an observable object that implements the IDisposable interface.
/// Provides a mechanism for managing and disposing of resources, such as event subscriptions.
/// </summary>
public abstract class KonkordObservableObject : ObservableObject, IDisposable
{
    /// <summary>
    /// Indicates whether the object has been disposed.
    /// </summary>
    private bool _isDisposed;

    /// <summary>
    /// A collection of IDisposable resources that will be disposed together.
    /// </summary>
    protected CompositeDisposable Disposables { get; } = new();

    /// <summary>
    /// Disposes the resources used by the object.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged and optionally managed resources used by the object.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether to release managed resources.
    /// If true, managed resources are released; otherwise, only unmanaged resources are released.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
            return;

        if (disposing)
        {
            // Dispose of all managed resources (like subscriptions).
            Disposables.Dispose();
        }

        _isDisposed = true;
    }
}
