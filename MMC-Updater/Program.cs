using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Microsoft.Extensions.Hosting;
using ReactiveUI.Avalonia;

namespace Tavstal.MesterMC.Updater;

// ReSharper disable once ClassNeverInstantiated.Global

/// <summary>
/// The entry point of the MMC-Updater application.
/// </summary>
[RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
class Program
{
    /// <summary>
    /// Gets the application host, which is used to configure and run the application.
    /// </summary>
    public static IHost? AppHost { get; private set; }
    
    /// <summary>
    /// The main entry point of the application.
    /// This method initializes the application host and starts the Avalonia application
    /// with a classic desktop lifetime.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        AppHost = Host.CreateDefaultBuilder(args)
            .Build();
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Configures the Avalonia application.
    /// This method is also used by the visual designer and should not be removed.
    /// </summary>
    /// <returns>An <see cref="AppBuilder"/> instance configured for the application.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}