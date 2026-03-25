using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Path = System.IO.Path;

namespace Tavstal.MesterMC.Installer.Views;

/// <summary>
/// Main window of the installer application.
/// Inherits from <see cref="KonkordWindow{TViewModel}"/> with <see cref="MainViewModel"/> as the view model type.
/// This partial class contains the constructor, event handlers and a helper method for folder picking.
/// </summary>
public partial class MainWindow : KonkordWindow<MainViewModel>
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainWindow));

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// The attribute <see cref="RequiresUnreferencedCodeAttribute"/> is preserved because this constructor
    /// may use code that could be trimmed/removed by the IL linker.
    /// </summary>
    [RequiresUnreferencedCode("This constructor uses code that may be removed during trimming.")]
    public MainWindow()
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        DataContext ??= new MainViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.CloseInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
            DataContext.DirPickerInteraction.RegisterHandler(async action =>
            {
                var result = await OpenDirPickerAsync();
                action.SetOutput(result);
            }).DisposeWith(disposables);
        });
        
        // Initial game directory
        DataContext.GameDirectory = OSHelper.GetHomeDirectory();
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                DataContext.GameDirectory = Path.Combine(DataContext.GameDirectory, ".mestermc");
                break;
            }
            default:
            {
                // Check for both "games" and "Games" directories for Linux and macOS
                string gamesDir = Directory.Exists(Path.Combine(DataContext.GameDirectory, "games"))
                    ? "games"
                    : "Games";
                DataContext.GameDirectory = Path.Combine(DataContext.GameDirectory, gamesDir, ".mestermc");
                break;
            }
        }
        
        // Initial start menu directory
        DataContext.StartMenuDirectory = OSHelper.GetProgramsDirectory();
    }

    #region Events

    /// <summary>
    /// Event handler for changes to the game directory text field.
    /// Validates the path and sets <see cref="MainViewModel.PathErrorMessage"/> accordingly.
    /// </summary>
    /// <param name="sender">Event sender (text box) or null.</param>
    /// <param name="e">Text changed event arguments.</param>
    private void GameDirPath_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext == null) return;
        if (!PathHelper.IsValidPath(DataContext.GameDirectory))
        {
            DataContext.PathErrorMessage = "A megadott játékkönyvtár nem érvényes.";
            return;
        }

        string gameDir = DataContext.GameDirectory!;
        string? rootPath = Path.GetPathRoot(gameDir);
        if (string.IsNullOrEmpty(rootPath) || rootPath.Equals(gameDir, StringComparison.OrdinalIgnoreCase))
        {
            DataContext.PathErrorMessage = "A megadott játékkönyvtár a gyökér könyvtár, ami nem érvényes.";
            return;
        }

        if (DataContext.HomeDirectory.Equals(gameDir, StringComparison.OrdinalIgnoreCase))
        {
            DataContext.PathErrorMessage = "A megadott játékkönyvtár nem lehet a 'home' könyvtár.";
            return;
        }

        if (DataContext.DesktopDirectory.Equals(gameDir, StringComparison.OrdinalIgnoreCase))
        {
            DataContext.PathErrorMessage = "A megadott játékkönyvtár nem lehet az 'asztal' könyvtár.";
            return;
        }

        if (DataContext.StartmenuDirectory.Equals(gameDir, StringComparison.OrdinalIgnoreCase))
        {
            DataContext.PathErrorMessage = "A megadott játékkönyvtár nem lehet a 'start menü' könyvtár.";
            return;
        }

        if (!FileSystemHelper.HasEnoughFreeSpace(gameDir, 1L * 1024 * 1024 * 1024))
        {
            DataContext.PathErrorMessage =
                "A megadott játékkönyvtárban nincs elegendő szabad hely (legalább 1 GB szükséges).";
            return;
        }

        DataContext.PathErrorMessage = string.Empty;
    }

    /// <summary>
    /// Event handler for changes to the start menu path text field.
    /// Validates the path and sets <see cref="MainViewModel.PathErrorMessage"/> accordingly.
    /// </summary>
    /// <param name="sender">Event sender (text box) or null.</param>
    /// <param name="e">Text changed event arguments.</param>
    private void StartMenuPath_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext == null) return;
        
        if (!PathHelper.IsValidPath(DataContext.StartMenuDirectory))
        {
            DataContext.PathErrorMessage = "A megadott start menü  nem érvényes.";
            return;
        }

        string startMenuDir = DataContext.StartMenuDirectory!;
        string? rootPath = Path.GetPathRoot(startMenuDir);
        if (string.IsNullOrEmpty(rootPath) || rootPath.Equals(startMenuDir, StringComparison.OrdinalIgnoreCase))
        {
            DataContext.PathErrorMessage = "A megadott start menü könyvtár a gyökér könyvtár, ami nem érvényes.";
            return;
        }

        if (DataContext.HomeDirectory.Equals(startMenuDir, StringComparison.OrdinalIgnoreCase))
        {
            DataContext.PathErrorMessage = "A megadott start menü könyvtár nem lehet a 'home' könyvtár.";
            return;
        }

        if (DataContext.DesktopDirectory.Equals(startMenuDir, StringComparison.OrdinalIgnoreCase))
        {
            DataContext.PathErrorMessage = "A megadott start menü könyvtár nem lehet az 'asztal' könyvtár.";
            return;
        }
        
        DataContext.PathErrorMessage = string.Empty;
    }
    #endregion

    /// <summary>
    /// Opens a folder picker dialog and returns the selected folder's local path.
    /// </summary>
    /// <returns>
    /// A task that resolves to the selected folder's local file system path, or <c>null</c> if none was selected or the platform
    /// does not support folder picking.
    /// </returns>
    private async Task<string?> OpenDirPickerAsync()
    {
        // Ensure the VisualRoot is a TopLevel object
        if (VisualRoot is not TopLevel topLevel)
            return null;

        var storageProvider = topLevel.StorageProvider;

        // Check if folder picking is supported on the current platform
        if (!storageProvider.CanPickFolder)
        {
            _logger.Error("Folder picking is not supported on this platform.");
            return null;
        }
    
        // Configure folder picker options
        var options = new FolderPickerOpenOptions
        {
            Title = "Válassz egy mappát",
            AllowMultiple = false
        };

        // Open the folder picker dialog
        IReadOnlyList<IStorageFolder> folders = await storageProvider.OpenFolderPickerAsync(options);

        // Return null if no folders were selected
        if (!folders.Any())
            return null;
    
        // Get the first selected folder
        IStorageFolder? selectedFolder = folders.FirstOrDefault();
        if (selectedFolder == null)
        {
            _logger.Error("No folder was selected.");
            return null;
        }
    
        // Return the local path of the selected folder
        return selectedFolder.Path.LocalPath;
    }
}