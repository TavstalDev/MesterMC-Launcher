using System;
using Avalonia;
using ReactiveUI.Avalonia;

namespace Tavstal.MesterMC.Installer;

/// <summary>
/// Program entry point for the installer application.
/// </summary>
class Program
{
    /// <summary>
    /// Application entry point.
    /// </summary>
    /// <param name="args">Command-line arguments forwarded to Avalonia's lifetime starter.</param>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>
    /// Configures and returns the application's <see cref="AppBuilder"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="AppBuilder"/> configured with platform detection, fonts and logging.
    /// </returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}