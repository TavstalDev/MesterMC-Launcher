using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia;
using Microsoft.Extensions.Hosting;
using ReactiveUI.Avalonia;
using Tavstal.KonkordLauncher.Core.Helpers;

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
    /// Indicates whether the application was started in uninstaller mode.
    /// True when the "--uninstall" command-line argument was passed to the process.
    /// </summary>
    public static bool UninstallerMode { get; private set; }
    
    /// <summary>
    /// The main entry point of the application.
    /// This method initializes the application host and starts the Avalonia application
    /// with a classic desktop lifetime.
    /// </summary>
    /// <param name="args">Command-line arguments passed to the application.</param>
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--uninstall-prepare"))
        {
            string? executablePath = Environment.ProcessPath;
            if (executablePath == null || !File.Exists(executablePath))
            {
                Console.Error.WriteLine("Failed to determine the path of the updater executable.");
                Environment.Exit(1);
                return;
            }
            string tmpDir = Path.Combine(Path.GetTempPath(), "mmcuninstaller");
            Directory.CreateDirectory(tmpDir);
            string targetPath = Path.Combine(tmpDir, InstallHelper.GetUpdaterExecutableName());
            if (FileSystemHelper.IsFileLocked(targetPath))
            {
                // Exit because the uninstaller is already running
                Environment.Exit(0);
                return;
            }
            
            File.Copy(executablePath, targetPath, true);

            Process.Start(new ProcessStartInfo
            {
                FileName = targetPath,
                Arguments = "--uninstall",
                UseShellExecute = true
            });

            Environment.Exit(0);
            return;
        }
        
        UninstallerMode = args.Contains("--uninstall");

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