using System.Diagnostics.CodeAnalysis;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.MesterMC.Installer.Views;

namespace Tavstal.MesterMC.Installer;

/// <summary>
/// Application entry point for the Avalonia-based installer UI.
/// </summary>
public partial class App : Application
{
    private static readonly string _tmpDir = Path.Combine(Path.GetTempPath(), "mmcinstaller_" + Path.GetRandomFileName());
    public static string TmpDir => _tmpDir;
    
    /// <summary>
    /// Loads Avalonia XAML definitions for this application.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when Avalonia has finished framework initialization.
    /// </summary>
    [RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
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