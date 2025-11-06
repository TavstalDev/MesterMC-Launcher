using System.Collections.Generic;
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

public partial class MainWindow : KonkordWindow<MainViewModel>
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainWindow));
    
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
                this.Close();
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

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);
        if (DataContext == null) 
            return;
        
        if (Directory.Exists(DataContext.TmpDir))
            Directory.Delete(DataContext.TmpDir, true);
    }

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
    
    private void GameDirPath_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext == null) return;

        DataContext.PathErrorMessage = PathHelper.IsValidPath(DataContext.GameDirectory) ? string.Empty : "A megadott játékkönyvtár nem érvényes.";
    }

    private void StartMenuPath_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (DataContext == null) return;

        DataContext.PathErrorMessage = PathHelper.IsValidPath(DataContext.GameDirectory) ? string.Empty : "A megadott start menü könyvtár nem érvényes.";
    }
}