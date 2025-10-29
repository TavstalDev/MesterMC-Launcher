/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
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

public abstract class KonkordObservableObject : ObservableObject, IDisposable
{
    private bool _isDisposed;

    // A CompositeDisposable to store all IDisposable resources (e.g., event subscriptions).
    protected CompositeDisposable Disposables { get; } = new();
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
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