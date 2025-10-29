using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.MesterMC.Launcher.Models;
using Tavstal.MesterMC.Launcher.Views.Models;

namespace Tavstal.MesterMC.Launcher.Views;

public partial class UpdateWindow : KonkordWindow<UpdateViewModel>, IProgressReporter
{
    /// <summary>
    /// Logger instance for the StartupWindow class.
    /// </summary>
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(UpdateWindow));

    /// <summary>
    /// Delay in milliseconds for each validation step.
    /// </summary>
    private readonly int _stepDelay = 100;
    
    public UpdateWindow()
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif

        DataContext ??= new UpdateViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var settings = await LauncherHelper.GetLauncherSettingsAsync();

            // 0. Set initial status
            SetStatus("Indulás...");

            // 1. Validate Directory Structure
            SetStatus("A könyvtárszerkezet ellenőrzése...");
            await Task.Delay(_stepDelay);
            if (!ValidationHelper.ValidateDataFolder())
            {
                SetStatus("A könyvtárszerkezet ellenőrzése sikertelen.");
                return;
            }

            // 2. Validate Manifests
            SetStatus("A minecraft manifest ellenőrzése...");
            await Task.Delay(_stepDelay);
            if (!await ValidationHelper.ValidateManifests(this))
            {
                SetStatus("A minecraft manifest ellenőrzése sikertelen.");
                return;
            }

            // 3. Validate Java
            SetStatus("A Java ellenőrzése...");
            await Task.Delay(_stepDelay);
            var javaInstallations = JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath);
            bool wasJavaUpdated = false;
            int[] javaVersionsToDownload = [21];
            foreach (int javaVersion in javaVersionsToDownload)
            {
                var jdkResult = javaInstallations.FirstOrDefault(x => x.Major == javaVersion);
                if (jdkResult != null)
                    continue;

                Progress<double> progress = new Progress<double>();
                progress.ProgressChanged += (_, prog) =>
                {
                    SetStatus("Java " + javaVersion + " letöltése... " + prog.ToString("0.00") + "%");
                    SetProgress(prog);
                };
                await JavaHelper.DownloadJavaVersionAsync(javaVersion, settings.Launcher.JavaDirectoryPath, progress);
                wasJavaUpdated = true;
            }

            if (wasJavaUpdated)
            {
                if (OSHelper.GetOperatingSystem() != EOperatingSystem.Windows)
                {
                    string[] directories = Directory.GetDirectories(settings.Launcher.JavaDirectoryPath);
                    foreach (string directory in directories)
                    {
                        string javaExecutablePath = Path.Combine(directory, "bin", "java");
                        if (!File.Exists(javaExecutablePath))
                            continue;
                        if (!await FileSystemHelper.MakeExecutableAsync(javaExecutablePath))
                        {
                            SetStatus("Nem sikerült végrehajthatóvá tenni a Java fájlt.");
                            _logger.Error("Failed to make Java executable: " + javaExecutablePath);
                        }
                    }
                }

                javaInstallations = JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath, true);
            }

            if (string.IsNullOrEmpty(settings.Java.JavaPath) && javaInstallations.Count > 0)
            {
                settings.Java.JavaPath = javaInstallations[0].Path;
                await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, settings);
            }

            // 4. Check for Updates
            App.IsUpToDate = true;
            if (settings.Launcher.EnableAutomaticUpdates && DateTime.Now > settings.Launcher.NextUpdateCheck)
            {
                SetStatus("Frissítések keresése...");
                await Task.Delay(_stepDelay);

                settings.Launcher.NextUpdateCheck =
                    DateTime.Now.AddHours(settings.Launcher.UpdateInterval == 0 ? 1 : settings.Launcher.UpdateInterval);
                await JsonHelper.WriteJsonFileAsync(PathHelper.LauncherConfigPath, settings);

                if (await CheckUpdateAsync())
                {
                    App.IsUpToDate = false;
                    Close();
                    return;
                }

                _logger.Debug("No updates found, starting application...");
            }
            else
                App.IsUpToDate = !await CheckUpdateAsync(true);
            
            // 5. Construct minecraft instance and download assets
            // Since this launcher should support only one instance there will be no instance selection.
            // The original project was made to support multiple instances so there might be some leftover code related to that.
            SetStatus("Minecraft példány előkészítése...");
            var instance = App.createMinecraftInstance(this);
            await instance.Start(true); 

            // 6. Create servers.dat if not exists
            if (!await ValidationHelper.ValidateServersAsync())
            {
                _logger.Error("Failed to validate servers.dat");
            }
            
            // 7. Start Main Window
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            {
                SetStatus("Nem sikerült elindítani a fő ablakot.");
                _logger.Error(
                    "Failed to start main window: Application lifetime is not IClassicDesktopStyleApplicationLifetime");
                return;
            }

            var oldWindow = desktop.MainWindow;
            var newWindow = new LauncherWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            desktop.MainWindow = newWindow;
            newWindow.Show();
            if (oldWindow != null)
                oldWindow.Close();
            else
                Close();
        });
    }
    
    private async Task<bool> CheckUpdateAsync(bool justCheck = false)
    {
        try
        {
            // 1. Fetch the latest release information from GitHub
            var result = await HttpHelper.GetAsync(MesterMcEndpoints.LatestRelease);
            if (result == null)
            {
                SetStatus("Nem sikerült lekérdezni a legújabb frissítést.");
                _logger.Error("Failed to get latest release");
                return false;
            }

            if (!result.IsSuccessStatusCode)
            {
                SetStatus("Nem sikerült lekérdezni a legújabb frissítést.");
                _logger.Error("Failed to get latest release, status code: " + result.StatusCode);
                return false;
            }

            string json = await result.Content.ReadAsStringAsync();
            JObject jsonObject = JObject.Parse(json);
            string? latestVersion = jsonObject["tag_name"]?.ToString();

            // 2. Compare the current version with the latest version
            Version? currentVersion;
            object[] versionAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (versionAttributes.Length > 0)
            {
                AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)versionAttributes[0];
                currentVersion = new Version(informationalVersionAttribute.InformationalVersion);
            }
            else
                currentVersion = Assembly.GetExecutingAssembly().GetName().Version;

            if (latestVersion == null || currentVersion == null)
            {
                SetStatus("Nem sikerült ellenőrizni a frissítéseket.");
                _logger.Error("Failed to parse latest version or current version");
                return false;
            }

            var latestVer = new Version(latestVersion);
            _logger.Debug($"Comparing versions: current={currentVersion}, latest={latestVer}");
            if (currentVersion >= latestVer)
                return false;
            
            if (!justCheck)
                await UpdateLauncherAsync();
            return true;
        }
        catch (Exception ex)
        {
            SetStatus("Váratlan hiba történt a frissítések ellenőrzése közben.");
            _logger.Exc("Error while checking for updates");
            _logger.Error(ex);
            return false;
        }
    }
    
    private async Task UpdateLauncherAsync()
    {
        try
        {
            string fileName = "Updater";
            if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
                fileName += ".exe";
            else if (OSHelper.GetOperatingSystem() == EOperatingSystem.MacOS)
                fileName += ".app";
            
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), fileName)))
            {
                SetStatus("A frissítő fájl nem található.");
                _logger.Error("Updater file not found: " + fileName);
                return;
            }
            
            ProcessStartInfo processInfo = new ProcessStartInfo()
            {
                FileName = Path.Combine(Directory.GetCurrentDirectory(), fileName),
                UseShellExecute = true,
            };
            var process = Process.Start(processInfo);
            if (process == null)
            {
                SetStatus("Nem sikerült elindítani a frissítőt.");
            }
        }
        catch (Exception ex)
        {
            SetStatus("Váratlan hiba történt a frissítő elindítása közben.");
            _logger.Exc("Error while updating the launcher");
            _logger.Error(ex);
        }
    }

    #region IProgressReporter Implementation
    /// <summary>
    /// Sets the progress value for the startup process.
    /// </summary>
    /// <param name="progress">The progress value, typically between 0.0 and 1.0.</param>
    public void SetProgress(double progress)
    {
        if (DataContext == null)
            return;

        DataContext.Progress = progress;
    }

    /// <summary>
    /// Sets the status message for the startup process.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void SetStatus(string status)
    {
        if (DataContext == null)
            return;

        DataContext.ProgressText = status;
    }

    /// <summary>
    /// Sets the status message with optional arguments.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    /// <param name="args">Optional arguments for formatting the status message.</param>
    public void SetStatus(string status, params object[]? args)
    {
        if (DataContext == null)
            return;
        
        if (args == null || args.Length == 0)
        {
            DataContext.ProgressText = status;
            return;
        }

        DataContext.ProgressText = string.Format(status, args);
    }
    #endregion 
}