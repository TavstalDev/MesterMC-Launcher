using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Tavstal.KonkordLauncher.Common.Helpers;
using Tavstal.KonkordLauncher.Core.Encryption;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Instances;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.MesterMC.Launcher.Views;

namespace Tavstal.MesterMC.Launcher;

public partial class App : Application
{
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(App));
    private static MinecraftInstance? _instance;
    public static IServiceProvider? Services => Program.AppHost?.Services;
    
    #region Screen Size
    private static PixelSize _screenSize = new(1920, 1080);
    public static PixelSize ScreenSize => _screenSize;
    
    public static decimal ScreenWidth => _screenSize.Width;
    public static decimal ScreenHeight => _screenSize.Height;
    public static void SetScreenSize(PixelSize screenSize)
    {
        _screenSize = screenSize;
    }
    #endregion

    #region Versioning
    private static string _version = string.Empty;
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
            
            _version = currentVersion?.ToString() ?? "2.0.0";
            return _version;
        }
    }
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
    private static string _buidDate = string.Empty;
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
    public static bool? IsUpToDate { get; set; }
    #endregion
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        var dataProctionService = Services?.GetRequiredService<IDataProtectionProvider>();
        if (dataProctionService != null)
            EncryptionUtility.SetDataProtectionProvider(dataProctionService);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new UpdateWindow();
            //desktop.MainWindow = new ClientWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    public static MinecraftInstance createMinecraftInstance(IProgressReporter progressReporter)
    {
        if (_instance != null)
            return _instance;

        var launcherSettings = LauncherHelper.GetLauncherSettings();
        
        string wrapperCommand = launcherSettings.Misc.WrapperCommand;
        // Add gamemoderun if enabled
        if (launcherSettings.Misc.EnableFeralGameMode && !wrapperCommand.Contains("gamemoderun"))
            wrapperCommand = "gamemoderun " + wrapperCommand;

        // Add mangohud if enabled
        if (launcherSettings.Misc.EnableMangoHud && !wrapperCommand.Contains("mangohud"))
            wrapperCommand = "mangohud " + wrapperCommand;

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
            "1.21.5", 
            EMinecraftKind.FABRIC, 
            "0.17.3", 
            launcherSettings.Launcher.MinecraftDataDirectoryPath,
            launcherSettings.Misc.PreLaunchCommand, 
            wrapperCommand, 
            launcherSettings.Misc.PostExitCommand, 
            environmentVariables, 
            "play.mestermc.hu"
            );
        
        
        PathDetails pathDetails = new PathDetails(
            launcherSettings.Launcher.AssetsDirectoryPath, 
            launcherSettings.Launcher.CacheDirectoryPath, 
            launcherSettings.Launcher.LibrariesDirectoryPath,
            launcherSettings.Launcher.VersionsDirectoryPath, 
            launcherSettings.Launcher.GetVanillaManifestPath(), 
            launcherSettings.Launcher.GetFabricManifestPath(), 
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
        return _instance;
    }
    
    public static MinecraftInstance? getInstance() => _instance;
}