using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.MesterMC.Updater.Views.Models;

namespace Tavstal.MesterMC.Updater.Views;

public partial class UpdateWindow : KonkordWindow<UpdateViewModel>, IProgressReporter
{
    /// <summary>
    /// Logger instance for the StartupWindow class.
    /// </summary>
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(UpdateWindow));
    private readonly string _tmpDir;
    
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
        
        _tmpDir = Path.Combine(Path.GetTempPath(), "mmcupdater_" + Path.GetRandomFileName());
        if (!Directory.Exists(_tmpDir))
            Directory.CreateDirectory(_tmpDir);
    }
    
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Dispatcher.UIThread.Invoke(async () => await StartUpdateProcessAsync());
    }
    
    protected override void OnClosed(EventArgs e)
    {
        if (Directory.Exists(_tmpDir))
            FileSystemHelper.DeleteDirectory(_tmpDir);
        base.OnClosed(e);
    }
    
    private async Task StartUpdateProcessAsync()
    {
        // TODO: Test on all platforms
        string targetAssetName = string.Empty;
        bool isArm = OSHelper.IsArmBased();
        switch (OSHelper.GetOperatingSystem())
        {
            case EOperatingSystem.Windows:
            {
                targetAssetName = isArm ? "MMCLauncher_{0}_windows_arm.zip" : "MMCLauncher_{0}_windows_x64.zip";
                break;
            }
            case EOperatingSystem.Linux:
            {
                targetAssetName = isArm ? "MMCLauncher_{0}_linux_arm.tar.gz" : "MMCLauncher_{0}_linux_x64.tar.gz";
                break;
            }
            case EOperatingSystem.MacOS:
            {
                targetAssetName = isArm ? "MMCLauncher_{0}_mac_arm.tar.gz" : "MMCLauncher_{0}_mac_x64.tar.gz";
                break;
            }
        }

        // 0. Send http request to GitHub API to get the latest release info
        var response = await HttpHelper.GetStringAsync(MesterMcEndpoints.LatestRelease);
        if (string.IsNullOrEmpty(response))
        {
            SetStatus("Hiba a frissítés lekérése közben.");
            _logger.Error("Failed to fetch release info from GitHub API.");
            return;
        }

        JObject releaseObject = JObject.Parse(response);
        if (!releaseObject.TryGetValue("assets", out var assetsToken))
        {
            SetStatus("Hiba a frissítés lekérése közben.");
            _logger.Error("No assets found in the latest release.");
            return;
        }

        string? version = releaseObject.Value<string>("tag_name")?.TrimStart('v');
        if (string.IsNullOrEmpty(version))
        {
            SetStatus("Hiba a frissítés lekérése közben.");
            _logger.Error("Failed to determine the latest version from the release info.");
            return;
        }

        // Insert version into the target asset name
        targetAssetName = string.Format(targetAssetName, version);

        JArray assetsArray = (JArray)assetsToken;
        // Find the target asset
        string? downloadUrl = null;
        foreach (var asset in assetsArray)
        {
            if (asset["name"]?.ToString() == targetAssetName)
            {
                downloadUrl = asset["browser_download_url"]?.ToString() ?? string.Empty;
                break;
            }
        }

        if (string.IsNullOrEmpty(downloadUrl))
        {
            SetStatus("Hibás letöltési link.");
            _logger.Error($"No suitable asset found for the current OS and architecture. Asset name: {targetAssetName}");
            return;
        }

        // 1. Download the asset
        var progress = new Progress<double>(p =>
        {
            var percent = (int)(p * 100);
            if (percent > 100)
                percent = 100; // Cap at 100%
            SetStatus("Letöltés...", percent);
        });
        string targetFilePath = Path.Combine(_tmpDir, targetAssetName);
        await HttpHelper.DownloadFileAsync(downloadUrl, targetFilePath, progress);

        // 2. Extract the downloaded file to the temporary directory
        SetStatus("Kicsomagolás...", targetAssetName);
        string targetTempDir = Path.Combine(_tmpDir, "extracted");
        if (targetAssetName.EndsWith(".tar.gz"))
        {
            await using Stream inStream = File.OpenRead(targetFilePath);
            await using Stream gzipStream = new GZipInputStream(inStream);
            using TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
            tarArchive.ExtractContents(targetTempDir);
        }
        else
            ZipFile.ExtractToDirectory(targetFilePath, targetTempDir);

        // Remove Updater from the extracted files
        foreach (var file in Directory.GetFiles(targetTempDir))
        {
            if (file.Contains("Updater"))
                File.Delete(file);
        }

        string tempBinDir = Path.Combine(targetTempDir, "bin");
        if (Directory.Exists(tempBinDir))
        {
            foreach (var file in Directory.GetFiles(tempBinDir))
            {
                if (file.Contains("Updater"))
                    File.Delete(file);
            }
        }

        // 3. Move the extracted files to the application directory
        SetStatus("Alkalmazás...");
        FileSystemHelper.MoveDirectory(targetTempDir, PathHelper.ApplicationDir, true);

        // 4. Delete the temporary directory
        SetStatus("Tisztítás...");
        if (Directory.Exists(_tmpDir))
            FileSystemHelper.DeleteDirectory(_tmpDir);

        // 5. Restart the application
        SetStatus("Újraindítás...");
        string fileName = "MMC-Launcher";
        if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
            fileName += ".exe";
        else if (OSHelper.GetOperatingSystem() == EOperatingSystem.MacOS)
            fileName += ".app";
        string appPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        if (!File.Exists(appPath))
        {
            SetStatus("Nem sikerült újraindítani a launchert.");
            _logger.Error("Failed to restart the launcher.");
            return;
        }

        ProcessStartInfo processInfo = new ProcessStartInfo()
        {
            FileName = appPath,
            UseShellExecute = true,
        };
        Process.Start(processInfo);
        Close();
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