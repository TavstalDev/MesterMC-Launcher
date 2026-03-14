using System;
using Avalonia;
using Microsoft.Extensions.Hosting;
using ReactiveUI.Avalonia;

namespace Tavstal.MesterMC.Launcher;

/// <summary>
/// Application entry point for the Avalonia-based MMC Launcher.
/// </summary>
class Program
{
    /// <summary>
    /// The application host created from <see cref="Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(string[])"/>.
    /// </summary>
    public static IHost? AppHost { get; private set; }
    
    /// <summary>
    /// The program's main entry point.
    /// </summary>
    [STAThread]
    public static void Main(string[] args)
    {
        AppHost = Host.CreateDefaultBuilder(args)
            .Build();
        
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    /// <summary>
    /// Configures and returns an <see cref="Avalonia.AppBuilder"/> for the application.
    /// </summary>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}