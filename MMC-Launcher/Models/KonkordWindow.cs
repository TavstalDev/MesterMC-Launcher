/*
 * Copyright (c) 2025 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */


using System;
using Avalonia.Controls;
using ReactiveUI.Avalonia;

namespace Tavstal.MesterMC.Launcher.Models;

public abstract class KonkordWindow<TViewModel> : ReactiveWindow<TViewModel> where TViewModel : class
{
    public new TViewModel? DataContext
    {
        get => (TViewModel?)base.DataContext;
        set => base.DataContext = value;
    }
    
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosing(e);
    }
}