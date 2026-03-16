using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DiscordRPC;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.MesterMC.Launcher.Helpers;
using Tavstal.MesterMC.Launcher.Models.Config.DTOs;
using Tavstal.MesterMC.Launcher.Models.Json;
using Tavstal.MesterMC.Launcher.Views;

namespace Tavstal.MesterMC.Launcher;

// ReSharper disable once PartialTypeWithSinglePart

/// <summary>
/// Main Avalonia application class for the MesterMC launcher.
/// </summary>
/// <remarks>
/// This partial <see cref="Application"/> contains startup logic, version information,
/// screen size helpers, game instance creation and Discord Rich Presence helpers.
/// It is marked with <see cref="RequiresUnreferencedCodeAttribute"/> because some of the
/// reflection-based version and metadata lookups may be affected by trimming.
/// </remarks>
[RequiresUnreferencedCode("This method uses code that may be removed during trimming.")]
public partial class App : Application
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(App));
    private static MinecraftInstance? _instance;
    private static DiscordRpcClient? _rpcClient;
    
    #region Screen Size
    /// <summary>
    /// The screen size used by the launcher for resolution calculations.
    /// </summary>
    private static PixelSize _screenSize = new(1920, 1080);
    
    /// <summary>
    /// Returns the current screen size used by the launcher.
    /// </summary>
    public static PixelSize ScreenSize => _screenSize;
    
    /// <summary>
    /// Convenience accessor for the screen width.
    /// </summary>
    public static decimal ScreenWidth => _screenSize.Width;
    
    /// <summary>
    /// Convenience accessor for the screen height.
    /// </summary>
    public static decimal ScreenHeight => _screenSize.Height;
    
    /// <summary>
    /// Sets the screen size used by the launcher. This affects resolution calculations
    /// for newly created game instances.
    /// </summary>
    /// <param name="screenSize">New screen size to use.</param>
    public static void SetScreenSize(PixelSize screenSize)
    {
        _screenSize = screenSize;
    }
    #endregion

    #region Versioning
    /// <summary>
    /// Backing storage for the computed application version string.
    /// </summary>
    private static string _version = string.Empty;
    
    /// <summary>
    /// Returns the application's version as a string.
    /// </summary>
    public static string Version
    {
        get
        {
            if (!string.IsNullOrEmpty(_version))
                return _version;
            
            Version? currentVersion;
            object[] versionAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (versionAttributes.Length > 0)
            {
                AssemblyInformationalVersionAttribute informationalVersionAttribute = (AssemblyInformationalVersionAttribute)versionAttributes[0];
                currentVersion = new Version(informationalVersionAttribute.InformationalVersion);
            }
            else
                currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            
            _version = currentVersion?.ToString() ?? "0.0.1";
            return _version;
        }
    }
    
    /// <summary>
    /// Returns the branch identifier used for the build (dev in DEBUG, stable otherwise).
    /// </summary>
    public static string Branch
    {
        get
        {
#if  DEBUG
            return "dev";
#else 
            return "stable";
#endif
        }   
    }
    /// <summary>
    /// Backing storage for the build date string.
    /// </summary>
    private static string _buidDate = string.Empty;
    
    /// <summary>
    /// Returns the build date for the running assembly.
    /// </summary>
    public static string BuildDate
    {
        get
        {
            if (!string.IsNullOrEmpty(_buidDate))
                return _buidDate;
            
            object[] buildDateAttributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyMetadataAttribute), false);
            foreach (var attribute in buildDateAttributes)
            {
                if (attribute is AssemblyMetadataAttribute { Key: "BuildDate" } metadata)
                {
                    _buidDate = metadata.Value ?? DateTime.UtcNow.ToString("yyyy-MM-dd");
                    return _buidDate;
                }
            }

            _buidDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            return _buidDate;
        }
    }
    
    /// <summary>
    /// Nullable flag indicating whether the launcher is up to date. Null when unknown.
    /// </summary>
    public static bool? IsUpToDate { get; set; }
    #endregion
    
    /// <summary>
    /// Loads Avalonia XAML for the application.
    /// </summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Called when Avalonia framework initialization completes. This method sets the main window
    /// and attempts to initialize Discord Rich Presence.
    /// </summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new UpdateWindow();
            try
            {
                _rpcClient = new DiscordRpcClient("1440491989261877359");
                _rpcClient.Initialize();
                _rpcClient.SetPresence(new RichPresence
                {
                    Details = "Az indítóban",
                    Timestamps = Timestamps.Now,
                    Assets = new Assets
                    {
                        LargeImageKey = "logo",
                        LargeImageText = "MesterMC",
                    }
                });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    #region Game Instance
    /// <summary>
    /// Creates a configured <see cref="MinecraftInstance"/> (Fabric) using the launcher settings.
    /// </summary>
    /// <param name="progressReporter">
    /// Optional progress reporter used by the instance while setting up or launching the game.
    /// </param>
    /// <returns>A configured <see cref="MinecraftInstance"/> ready for setup/launch.</returns>
    public static MinecraftInstance createMinecraftInstance(IProgressReporter? progressReporter)
    {
        CoreConfigDto launcherSettings = LauncherHelper.GetLauncherSettings();

        // Attempt to force the use of a dedicated GPU if configured
        var environmentVariables = launcherSettings.EnableEnvironmentVariables
            ? launcherSettings.EnvironmentVariables
            : [];
        
        var gpuInfo = OSHelper.GetDedicatedGpuType();
        if (launcherSettings.Misc.UseDedicatedGpu && gpuInfo != null)
        {
            switch (OSHelper.GetOperatingSystem())
            {
                case EOperatingSystem.Windows:
                {
                    switch (gpuInfo.Value.Item1)
                    {
                        case "amd":
                        {
                            environmentVariables.Add("AMD_POWERXPRESS_REQUEST_HIGH_PERFORMANCE", "1");
                            break;
                        }
                        case "nvidia":
                        {
                            environmentVariables.Add("__NV_GPU_USE_DISCRETE_GPU", "1");
                            break;
                        }
                    }
                    break;
                }
                case EOperatingSystem.Linux:
                {
                    switch (gpuInfo.Value.Item1)
                    {
                        case "amd":
                        case "intel":
                        {
                            environmentVariables.Add("DRI_PRIME", "1");
                            break;
                        }
                        case "nvidia":
                        {
                            environmentVariables.Add("__NV_PRIME_RENDER_OFFLOAD", "1");
                            environmentVariables.Add("__GLX_VENDOR_LIBRARY_NAME", "nvidia");
                            break;
                        }
                    }
                    break;
                }
            }
        }
        
        // Add custom native libraries if configured
        List<string> nativeLibraries = [];
        if (launcherSettings.Misc.UseCustomGlfw && File.Exists(launcherSettings.Misc.CustomGlfwPath))
            nativeLibraries.Add(launcherSettings.Misc.CustomGlfwPath);
        if (launcherSettings.Misc.UseCustomOpenAl && File.Exists(launcherSettings.Misc.CustomOpenAlPath))
            nativeLibraries.Add(launcherSettings.Misc.CustomOpenAlPath);

        GameDetails gameDetails = new GameDetails(
            launcherSettings.Java.JavaPath, 
            launcherSettings.Java.MinMemory, 
            launcherSettings.Java.MaxMemory, 
            launcherSettings.Java.JvmArguments, 
            "1.21.8", 
            EMinecraftKind.FABRIC, 
            "0.17.3", 
            launcherSettings.Launcher.MinecraftDataDirectoryPath,
            launcherSettings.Misc.EnableFeralGameMode,
            launcherSettings.Misc.EnableMangoHud,
            environmentVariables, 
            null
            );
        
        
        PathDetails pathDetails = new PathDetails(
            launcherSettings.Launcher.AssetsDirectoryPath, 
            launcherSettings.Launcher.CacheDirectoryPath, 
            launcherSettings.Launcher.LibrariesDirectoryPath,
            nativeLibraries);
        LauncherDetails launcherDetails = new LauncherDetails("MesterMC", Version);
        ClientDetails clientDetails = new ClientDetails("0", "teszt", GameHelper.GetOfflinePlayerUUID("teszt"), true); // Default details, it will be updated later in the process
        var resolution = new Resolution(
            launcherSettings.Minecraft.StartMaximized
                ? (uint)ScreenSize.Width
                : launcherSettings.Minecraft.WindowWidth,
            launcherSettings.Minecraft.StartMaximized
                ? (uint)ScreenSize.Height
                : launcherSettings.Minecraft.WindowHeight
        );

        _instance = new FabricInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter);
        _instance.OnSetupDefaultJava += SetupDefaultJavaPath;
        return _instance;
    }
    
    /// <summary>
    /// Callback invoked by the game instance to suggest a default Java path based on version metadata.
    /// </summary>
    /// <param name="meta">The Java version metadata provided by the game or mod loader.</param>
    private static void SetupDefaultJavaPath(VersionMeta? meta)
    {
        var settings = LauncherHelper.GetLauncherSettings();
        string defaultJavaPath = settings.Java.JavaPath;
        if (meta == null)
        {
            _logger.Warn("Java version meta is null, using existing Java path.");
            UpdateJavaPath(defaultJavaPath);
            return;
        }
        
        // Check if the Java version specified in the metadata is available, if not attempt to download it
        var javaInstallations = JavaHelper.LocateJavaInstallations(settings.Launcher.JavaDirectoryPath, false, true);
        if (javaInstallations.All(x => x.Major != meta.JavaVersionMeta.MajorVersion) && string.IsNullOrEmpty(defaultJavaPath))
        {
            _logger.Error("Required Java version not found and no default Java path set.");
            return;
        }

        foreach (var javaInstallation in javaInstallations)
        {
            if (meta.JavaVersionMeta != null && javaInstallation.Major == meta.JavaVersionMeta.MajorVersion)
            {
                defaultJavaPath = javaInstallation.Path;
                break;
            }
        }
        UpdateJavaPath(defaultJavaPath);
    }
    
    /// <summary>
    /// Updates the stored Java path in both the running instance and the launcher settings file.
    /// </summary>
    /// <param name="javaPath">Path to the Java executable or home to use for launching Minecraft.</param>
    private static void UpdateJavaPath(string javaPath)
    {
        _instance?.UpdateJavaPath(javaPath);
        var settings = LauncherHelper.GetLauncherSettings();
        settings.Java.JavaPath = javaPath;
        JsonHelper.WriteJsonFile(PathHelper.LauncherConfigPath, settings, CustomJsonContext.Default.CoreConfigDto);
    }
    #endregion
    
    #region Discord RPC
    /// <summary>
    /// Updates the Discord Rich Presence (RPC) with the specified details.
    /// </summary>
    /// <param name="details">The details to display in the Discord Rich Presence.</param>
    public static void UpdateRPC(string details)
    {
        try
        {
            // Check if the Discord RPC client is initialized and not disposed.
            if (_rpcClient == null || _rpcClient.IsDisposed)
            {
                _logger.Error("Discord RPC client is not initialized or disposed.");
                return;
            }
        
            // Set the presence with the provided details and current timestamps.
            _rpcClient.SetPresence(new RichPresence
            {
                Details = details,
                Timestamps = _rpcClient?.CurrentPresence?.Timestamps ?? Timestamps.Now
            });
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the update process.
            _logger.Exc("Failed to update Discord RPC");
            _logger.Error(ex);
        }
    }

    /// <summary>
    /// Clears the Discord Rich Presence (RPC) by removing the current presence.
    /// </summary>
    public static void ClearRPC()
    {
        try
        {
            // Check if the Discord RPC client is initialized and not disposed.
            if (_rpcClient == null || _rpcClient.IsDisposed)
            {
                _logger.Error("Discord RPC client is not initialized or disposed.");
                return;
            }

            // Clear the presence asynchronously.
            Task.Run(() =>
            {
                _rpcClient.SetPresence(null);
                _rpcClient.Invoke();
            });
        }
        catch (Exception ex)
        {
            // Log any exceptions that occur during the clearing process.
            _logger.Exc("Failed to clear Discord RPC");
            _logger.Error(ex);
        }
    }
    #endregion
}