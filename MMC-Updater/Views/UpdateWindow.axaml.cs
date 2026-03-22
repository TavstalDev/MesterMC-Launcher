using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
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

/// <summary>
/// Represents the main update window for the MMC-Updater application.
/// Implements the <see cref="IProgressReporter"/> interface to report progress updates.
/// </summary>
public partial class UpdateWindow : KonkordWindow<UpdateViewModel>, IProgressReporter
{
    /// <summary>
    /// Logger instance for the StartupWindow class.
    /// </summary>
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(UpdateWindow));
    
    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWindow"/> class.
    /// Sets up the data context, event handlers, and temporary directory.
    /// </summary>
    [RequiresUnreferencedCode("This constructor uses code that may be removed during trimming.")]
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
        
        FitToDisplay();
    }
    
    #region Window Events
    /// <summary>
    /// Handles the event when the window is opened.
    /// Starts the update process asynchronously.
    /// </summary>
    /// <param name="e">Event arguments for the opened event.</param>
    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        Dispatcher.UIThread.Invoke(async () => await StartUpdateProcessAsync());
    }
    
    /// <summary>
    /// Handles the pointer pressed event to enable dragging the window.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">Pointer pressed event arguments.</param>
    private void DragStart_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Start moving the window when left mouse button is pressed
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            BeginMoveDrag(e);
    }
    #endregion
    
    #region Private Methods
    /// <summary>
    /// Starts the update process asynchronously.
    /// This method handles downloading, extracting, and applying updates.
    /// </summary>
    private async Task StartUpdateProcessAsync()
    {
        // TODO: Test on all platforms
        string backupPath = Path.Combine(App.TmpDir, "updateBackup.zip");
        bool backupCreated = false;
        bool success = false;
        string? executablePath = null;
        // The updater expects it to be in the same directory as the launcher executable
        string applicationDir = Directory.GetCurrentDirectory();

        try
        {
            if (!await FileSystemHelper.HasWritePermissionAsync(App.TmpDir) ||
                !await FileSystemHelper.HasWritePermissionAsync(applicationDir))
            {
                SetStatus("Nincs írási jogosultság a szükséges könyvtárakban.");
                _logger.Error("Insufficient write permissions for the temporary or application directory.");
                return;
            }

            #region Get Latest Release

            // Get the base asset name
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

            // Send http request to GitHub API to get the latest release info
            var response = await HttpHelper.GetStringAsync(MesterMcEndpoints.LatestRelease);
            if (string.IsNullOrEmpty(response))
            {
                SetStatus("Hiba a frissítés lekérése közben.");
                _logger.Error("Failed to fetch release info from GitHub API.");
                return;
            }

            // Get the json response
            JObject releaseObject = JObject.Parse(response);
            if (!releaseObject.TryGetValue("assets", out var assetsToken))
            {
                SetStatus("Hiba a frissítés lekérése közben.");
                _logger.Error("No assets found in the latest release.");
                return;
            }

            // Get the version
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
            string? digest = null;
            foreach (var asset in assetsArray)
            {
                if (asset["name"]?.ToString() == targetAssetName)
                {
                    downloadUrl = asset["browser_download_url"]?.ToString() ?? string.Empty;
                    digest = asset["digest"]?.ToString() ?? string.Empty;
                    break;
                }
            }

            if (string.IsNullOrEmpty(downloadUrl) || string.IsNullOrEmpty(digest))
            {
                SetStatus("Hibás letöltési link.");
                _logger.Error(
                    $"No suitable asset found for the current OS and architecture. Asset name: {targetAssetName}");
                return;
            }

            #endregion

            #region Backup

            // Create a backup of the current application directory
            SetStatus("Biztonsági mentés készítése...");
            try
            {
                ZipFile.CreateFromDirectory(applicationDir, backupPath);
                backupCreated = true;
            }
            catch (Exception be)
            {
                SetStatus("Nem sikerült biztonsági mentést készíteni.");
                _logger.Error("Failed to create backup of the current application directory: \n" + be);
            }

            #endregion

            #region Downloading and Extracting
            // 1. Download the asset
            var progress = new Progress<double>(p =>
            {
                var percent = (int)(p * 100);
                if (percent > 100)
                    percent = 100; // Cap at 100%
                SetStatus("Letöltés...", percent);
            });
            string targetFilePath = Path.Combine(App.TmpDir, targetAssetName);
            await HttpHelper.DownloadFileAsync(downloadUrl, targetFilePath, progress);
            
            // 2. Check integrity
            if (!FileSystemHelper.CheckByDigest(targetFilePath, digest))
            {
                SetStatus("Nem sikerült a fájl integritásának ellenőrzése.");
                _logger.Error("File integrity check failed for the downloaded asset.");
                return;
            }

            // 3. Extract the downloaded file to the temporary directory
            SetStatus("Kicsomagolás...", targetAssetName);
            string targetTempDir = Path.Combine(App.TmpDir, "extracted");
            if (targetAssetName.EndsWith(".tar.gz"))
            {
                await using Stream inStream = File.OpenRead(targetFilePath);
                await using Stream gzipStream = new GZipInputStream(inStream);
                using TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8);
                tarArchive.ExtractContents(targetTempDir);
            }
            else
                ZipFile.ExtractToDirectory(targetFilePath, targetTempDir, true);

            // 4. Remove Updater from the extracted files
            foreach (var file in Directory.GetFiles(targetTempDir))
                if (file.Contains("Updater"))
                    File.Delete(file);

            string tempBinDir = Path.Combine(targetTempDir, "bin");
            if (Directory.Exists(tempBinDir))
                foreach (var file in Directory.GetFiles(tempBinDir))
                    if (file.Contains("Updater"))
                        File.Delete(file);
            #endregion

            // 3. Move the extracted files to the application directory
            SetStatus("Alkalmazás...");
            FileSystemHelper.MoveDirectory(targetTempDir, applicationDir, true);

            // 4. Delete the temporary directory
            SetStatus("Tisztítás...");
            if (Directory.Exists(App.TmpDir))
                FileSystemHelper.DeleteDirectory(App.TmpDir);

            // 5. Restart the application
            SetStatus("Újraindítás...");
            string fileName = "MMC-Launcher";
            if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
                fileName += ".exe";

            executablePath = Path.Combine(applicationDir, fileName);
            if (!File.Exists(executablePath))
            {
                SetStatus("Nem sikerült újraindítani a launchert.");
                _logger.Error("Failed to restart the launcher.");
                return;
            }

            success = true;
        }
        catch (Exception ex)
        {
            SetStatus("Hiba történt a frissítés során.");
            _logger.Error("Unexpected error during the update process: \n" + ex);
        }
        finally
        {
            try
            {
                // Restore backup
                if (!success && backupCreated)
                    ZipFile.ExtractToDirectory(backupPath, applicationDir, true);
                if (backupCreated)
                    File.Delete(backupPath);
                if (success && !string.IsNullOrEmpty(executablePath))
                {
                    ProcessStartInfo processInfo = new ProcessStartInfo()
                    {
                        FileName = executablePath,
                        UseShellExecute = true,
                    };
                    Process.Start(processInfo);
                    Close();
                }
            }
            catch (Exception fe)
            {
                SetStatus("Nem várt hiba a frissítés véglesítése közben.");
                _logger.Error("Unexpected error during update finalization: \n" + fe);
            }
        }
    }
    
    /// <summary>
    /// Adjusts the window size and position to fit the display.
    /// </summary>
    private void FitToDisplay()
    {
        double screenWidth = (double)App.ScreenWidth;
        double screenHeight = (double)App.ScreenHeight;
        
        // Base design resolution
        const double baseWidth = 706;
        const double baseHeight = 597;

        // Calculate scale factor relative to screen
        double scaleX = screenWidth / baseWidth;
        double scaleY = screenHeight / baseHeight;
        double scale = Math.Min(scaleX, scaleY) * 0.6;

        // Apply scaled window size
        Width = baseWidth * scale;
        Height = baseHeight * scale;

        // Center window on screen
        Position = new PixelPoint(
            (int)((screenWidth - Width) / 2),
            (int)((screenHeight - Height) / 2)
        );
    }
    #endregion
    
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