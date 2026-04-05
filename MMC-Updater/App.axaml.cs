using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.MesterMC.Updater.Views;

namespace Tavstal.MesterMC.Updater;

// ReSharper disable once PartialTypeWithSinglePart - AvaloniaUI requires the App class to be partial.

/// <summary>
/// Represents the main application class for the MMC-Updater.
/// </summary>
[RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
public partial class App : Application
{
    #region Screen Size
    /// <summary>
    /// Stores the screen size as a <see cref="PixelSize"/> object.
    /// Default value is 1920x1080.
    /// </summary>
    private static PixelSize _screenSize = new(1920, 1080);
    
    /// <summary>
    /// Gets the current screen size.
    /// </summary>
    public static PixelSize ScreenSize => _screenSize;
    
    /// <summary>
    /// Gets the width of the screen as a decimal value.
    /// </summary>
    public static decimal ScreenWidth => _screenSize.Width;
    
    /// <summary>
    /// Gets the height of the screen as a decimal value.
    /// </summary>
    public static decimal ScreenHeight => _screenSize.Height;
    
    /// <summary>
    /// Sets the screen size to the specified <see cref="PixelSize"/>.
    /// </summary>
    /// <param name="screenSize">The new screen size to set.</param>
    public static void SetScreenSize(PixelSize screenSize)
    {
        _screenSize = screenSize;
    }
    #endregion

    /// <summary>
    /// Temporary directory used during the update process.
    /// </summary>
    private static readonly string _tmpDir = Path.Combine(Path.GetTempPath(), "mmcupdater_" + Path.GetRandomFileName());
    public static string TmpDir => _tmpDir;
    
    /// <summary>
    /// Initializes the application by loading the XAML resources.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        try
        {
            if (!Directory.Exists(_tmpDir))
                Directory.CreateDirectory(_tmpDir);
        }
        catch
        {
            // Ignore any exceptions that occur during temporary directory creation
        }
    }
    
    /// <summary>
    /// Called when the framework initialization is completed.
    /// Sets the main window of the application to an instance of <see cref="UpdateWindow"/> or <see cref="UninstallWindow"/>.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Program.UninstallerMode ? new UninstallWindow() : new UpdateWindow();
            desktop.Exit += (_, _) =>
            {
                try
                {
                    if (Directory.Exists(TmpDir))
                        FileSystemHelper.DeleteDirectory(TmpDir);
                }
                catch
                {
                    // Ignore any exceptions that occur during cleanup
                }
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
}