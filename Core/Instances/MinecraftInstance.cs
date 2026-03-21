/*
 * Copyright (c) 2025-2026 Zoltan 'Tavstal' Solymosi. All rights reserved.
 * * This file is sourced from: https://github.com/TavstalDev/KonkordLauncher
 * * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * * For full license details, see <http://www.gnu.org/licenses/gpl-3.0.html>.
 */

using System.Diagnostics;
using Tavstal.KonkordLauncher.Core.Enums;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Services;

namespace Tavstal.KonkordLauncher.Core.Instances;

/// <summary>
/// Represents a Minecraft instance, handling installation, configuration, and launching of the game.
/// </summary>
public class MinecraftInstance
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MinecraftInstance));
    private readonly LauncherDetails _launcherDetails;
    private readonly ClientDetails _client;
    private readonly bool _isInitialized;
    private string _classPathFilePath = string.Empty;

    protected GameDetails GameDetails { get; }
    protected PathDetails PathDetails { get; }
    protected Resolution? Resolution { get; }
    protected VersionDetails VersionData { get; }
    public VersionManifest? VersionManifest { get; }
    public MinecraftVersion? MinecraftVersion { get; }
    protected IProgressReporter? _progressReporter { get; }

    protected VersionMeta? MinecraftVersionMeta { get; private set; }
    protected readonly List<string> _classPath = [];
    protected readonly List<LaunchArg> _jvmArguments = [];
    protected readonly List<LaunchArg> _gameArguments = [];
    protected readonly List<LaunchArg> _jvmArgumentsBeforeClassPath = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MinecraftInstance"/> class.
    /// </summary>
    /// <param name="gameDetails">Details about the game being installed.</param>
    /// <param name="pathDetails">Details about the file paths used for installation.</param>
    /// <param name="launcherDetails">Details about the launcher being used.</param>
    /// <param name="clientDetails">Details about the client user.</param>
    /// <param name="resolution">Optional resolution settings for the game.</param>
    /// <param name="progressReporter">Optional progress reporter for tracking installation progress.</param>
    protected MinecraftInstance(GameDetails gameDetails, PathDetails pathDetails, LauncherDetails launcherDetails,
        ClientDetails clientDetails, Resolution? resolution = null, IProgressReporter? progressReporter = null)
    {
        _progressReporter = progressReporter;
        GameDetails = gameDetails;
        PathDetails = pathDetails;
        Resolution = resolution;
        _launcherDetails = launcherDetails;
        _client = clientDetails;
        VersionData = new VersionDetails
        {
            CustomVersion = GameDetails.CustomVersion!,
            MinecraftVersion = GameDetails.MinecraftVersion,
            GameDir = GameDetails.CustomGameDirectory,
            NativesDir = Path.Combine(GameDetails.CustomGameDirectory, "natives")
        };

        var versionManifest = ManifestHelper.GetMinecraftManifest();
        if (versionManifest == null)
            throw new Exception("Minecraft version manifest not found.");
        VersionManifest = versionManifest;

        var minecraftVersion = VersionManifest.Versions.FirstOrDefault(x => x.Id == GameDetails.MinecraftVersion);
        if (minecraftVersion == null)
            throw new Exception($"Minecraft version {GameDetails.MinecraftVersion} not found in manifest.");
        MinecraftVersion = minecraftVersion;
        _isInitialized = true;
    }

    /// <summary>
    /// Update stored client details for this instance.
    /// </summary>
    /// <param name="clientDetails">New client details to apply; the method copies properties from this object into the internal <c>_client</c> instance.</param>
    public void UpdateUserDetails(ClientDetails clientDetails)
    {
        _client.DisplayName = clientDetails.DisplayName;
        _client.UUID = clientDetails.UUID;
        _client.AccessToken = clientDetails.AccessToken;
        _client.ClientId = clientDetails.ClientId;
        _client.Xuid = clientDetails.Xuid;
        _client.IsOffline = clientDetails.IsOffline;
    }

    /// <summary>
    /// Starts the Minecraft installation and launches the game.
    /// </summary>
    /// <returns>A <see cref="Process"/> object representing the launched game, or null if the process fails.</returns>
    public async Task<Process?> Start(bool downloadOnly = false)
    {
        if (!_isInitialized)
        {
            _logger.Error("Starting minecraft instance before a successful initialization.");
            return null;
        }
        
        _logger.Debug("Starting minecraft instance...");
        _jvmArguments.Clear();
        _gameArguments.Clear();
        _jvmArgumentsBeforeClassPath.Clear();
        _classPath.Clear();
        _logger.Debug("Setting up directories...");
        string tempDir = Path.Combine(Path.GetTempPath(), "mmclauncher_" + Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            _logger.Debug("Downloading core files...");
            DateTime startTime = DateTime.Now;
            await DownloadCoreFilesAsync();
            DateTime endTime = DateTime.Now;
            _logger.Info($"Core files downloaded in {(endTime - startTime).TotalMilliseconds}ms.");

            _logger.Debug("Installing modded data if applicable...");
            startTime = DateTime.Now;
            var moddedData = await InstallModdedAsync(tempDir);
            endTime = DateTime.Now;
            _logger.Info($"Modded data installation completed in {(endTime - startTime).TotalMilliseconds}ms.");
            _logger.Debug("Getting launch parameters...");
            var (versionDetails, mainClass, customVersion) = GetLaunchParameters(moddedData);

            _logger.Debug("Preparing game directory...");
            if (!Directory.Exists(versionDetails.GameDir))
                Directory.CreateDirectory(versionDetails.GameDir);

            _logger.Debug("Getting combined libraries...");
            var libraries = GetCombinedLibraries(moddedData);
            _logger.Debug("Downloading dependencies...");
            startTime = DateTime.Now;
            await DownloadDependenciesAsync(versionDetails, libraries);
            endTime = DateTime.Now;
            _logger.Info($"Dependencies downloaded in {(endTime - startTime).TotalMilliseconds}ms.");

            _classPath.Add(moddedData != null ? moddedData.VersionData.VersionJarPath : VersionData.VersionJarPath);

            _logger.Debug("Building arguments...");
            var args = BuildArguments(versionDetails.GameDir, mainClass, versionDetails.NativesDir, customVersion);
            _progressReporter?.Hide();

            // Copy custom natives if specified
            _logger.Debug("Copying custom native files if specified...");
            startTime = DateTime.Now;
            foreach (string nativePath in PathDetails.CustomNativeFiles)
            {
                if (!File.Exists(nativePath))
                    continue;
                string destPath = Path.Combine(versionDetails.NativesDir, Path.GetFileName(nativePath));
                File.Copy(nativePath, destPath, true);
            }

            endTime = DateTime.Now;
            _logger.Info($"Custom native files copied in {(endTime - startTime).TotalMilliseconds}ms.");

            _logger.Debug("The process is ready to launch.");
            if (downloadOnly)
            {
                _logger.Debug("Download only flag is set. Exiting before launch.");
                return null;
            }

            // Check mods
            _logger.Debug("Verifying mods...");
            startTime = DateTime.Now;
            ModService.VerifyMods(Path.Combine(versionDetails.GameDir, "mods"));
            await ModService.VerifyCustomSkinLoaderConfigAsync(versionDetails.GameDir);
            endTime = DateTime.Now;
            _logger.Info($"Mods verified in {(endTime - startTime).TotalMilliseconds}ms.");

            // Make commands_history readonly
            _logger.Debug("Attempting to fix command history file leak...");
            FileSystemHelper.FixCommandHistoryFile(GameDetails.CustomGameDirectory);

            // Launch the Minecraft game process with the constructed arguments
            _logger.Debug("Starting java virtual machine...");
            var process = JavaProcessLauncher.StartJava(GameDetails.JavaPath, args.Item1!, args.Item2!,
                GameDetails.EnableGamemode, GameDetails.EnableMangoHud, GameDetails.EnvironmentVariables);

            _logger.Debug("Deleting temporary directory...");
            FileSystemHelper.DeleteDirectory(tempDir);
            return process;
        }
        catch (Exception ex)
        {
            _logger.Error($"An error occurred while starting the Minecraft instance: {ex}");
            return null;
        }
        finally
        {
            if (Directory.Exists(tempDir))
                FileSystemHelper.DeleteDirectory(tempDir);
        }
    }

    /// <summary>
    /// Downloads the core files required for the Minecraft installation.
    /// </summary>
    private async Task DownloadCoreFilesAsync()
    {
        if (!_isInitialized)
        {
            _logger.Error("Attempted to download core files before successful initialization.");
            return;
        }

        if (MinecraftVersion == null)
        {
            _logger.Error("MinecraftVersion is null. Cannot download core files.");
            return;
        }
        
        var res = await MinecraftFileService.DownloadVersionAsync(MinecraftVersion, PathDetails, _progressReporter);
        if (res == null)
            throw new InvalidOperationException("Failed to download the version meta data. Please check your internet connection and try again.");
        MinecraftVersionMeta = res.Value.Item1;
        VersionData.VersionJarPath = res.Value.Item3;
        VersionData.VersionJsonPath = res.Value.Item2;
        
        if (MinecraftVersionMeta == null)
        {
            _logger.Error("MinecraftVersionMeta is null. Cannot download core files.");
            return;
        }

        // Change the required Java version if necessary
        if (GameDetails.Kind == EMinecraftKind.FORGE)
        {
            Version forgeMinecraftVersion = new Version(GameDetails.MinecraftVersion);
            // Set the required Java version to 7 for Forge versions 1.7.2 and below
            if (forgeMinecraftVersion.Major == 1 &&
                (forgeMinecraftVersion.Minor < 7 || forgeMinecraftVersion is { Minor: 7, Build: < 10 }))
                MinecraftVersionMeta.JavaVersionMeta.MajorVersion = 7;
        }
        
        await MinecraftFileService.DownloadMappingsAsync(MinecraftVersionMeta, PathDetails.AssetsDir, _progressReporter);
        await MinecraftFileService.DownloadAssetsAsync(MinecraftVersionMeta, PathDetails.AssetsDir, VersionData.GameDir, _progressReporter);
    }

    /// <summary>
    /// Retrieves the launch parameters for the Minecraft process.
    /// </summary>
    /// <param name="moddedData">Optional modded data for the installation.</param>
    /// <returns>A tuple containing version details, the main class, and the custom version string.</returns>
    private (VersionDetails, string, string?) GetLaunchParameters(ModdedData? moddedData)
    {
        if (MinecraftVersionMeta == null)
            throw new InvalidOperationException("MinecraftVersionMeta is null. Cannot download launch parameters.");
        
        if (moddedData != null)
            return (moddedData.VersionData, moddedData.MainClass ?? MinecraftVersionMeta.MainClass, moddedData.VersionData.CustomVersion);
        return (VersionData, MinecraftVersionMeta.MainClass, null);
    }

    /// <summary>
    /// Combines the libraries required for the installation, including modded libraries if applicable.
    /// </summary>
    /// <param name="moddedData">Optional modded data for the installation.</param>
    /// <returns>A list of combined library metadata.</returns>
    private List<LibraryMeta> GetCombinedLibraries(ModdedData? moddedData)
    {
        if (MinecraftVersionMeta == null)
        {
            _logger.Error("MinecraftVersionMeta is null. Cannot download combined libraries.");
            return [];
        }
        
        var libraries = new List<LibraryMeta>(MinecraftVersionMeta.Libraries);
        if (moddedData?.Libraries.Count > 0)
            libraries.InsertRange(0, moddedData.Libraries);
        return libraries;
    }

    /// <summary>
    /// Downloads the dependencies required for the Minecraft installation.
    /// </summary>
    /// <param name="versionDetails">The version details of the installation.</param>
    /// <param name="libraries">The list of libraries to download.</param>
    /// <returns>A list of native libraries required for the installation.</returns>
    private async Task DownloadDependenciesAsync(VersionDetails versionDetails, List<LibraryMeta> libraries)
    {
        if (MinecraftVersionMeta == null)
        {
            _logger.Error("MinecraftVersionMeta is null. Cannot download dependencies.");
            return;
        }
        
        _logger.Debug("Downloading logging...");
        var loggingArg = await MinecraftFileService.DownloadLoggingAsync(MinecraftVersionMeta, versionDetails.GameDir, _progressReporter);
        if (loggingArg != null)
            _jvmArgumentsBeforeClassPath.Add(loggingArg);

        _logger.Debug("Downloading libraries...");
        var classPath = await MinecraftFileService.DownloadLibrariesAsync(GameDetails.Kind, VersionData, libraries, _classPath, PathDetails.CacheDir, PathDetails.LibrariesDir, _progressReporter);
        foreach (var cp in classPath)
        {
            if (_classPath.Contains(cp))
                continue;
            _classPath.Add(cp);
        }
        
        _logger.Debug("Downloading launch wrapper...");
        string? result = await MinecraftFileService.DownloadLaunchWrapperAsync(PathDetails.LibrariesDir, _progressReporter);
        if (!string.IsNullOrEmpty(result))
            _classPath.Add(result);
    }

    /// <summary>
    /// Installs modded data for the Minecraft installation. This method can be overridden by derived classes.
    /// </summary>
    /// <param name="tempDir">The temporary directory used for installation.</param>
    /// <returns>A task representing the asynchronous operation, returning modded data if applicable.</returns>
    protected virtual Task<ModdedData?> InstallModdedAsync(string tempDir)
    {
        // Vanilla installer, do nothing
        return Task.FromResult<ModdedData?>(null);
    }

    /// <summary>
    /// Builds the JVM and game argument strings required to launch Minecraft and writes a classpath file to the game directory.
    /// </summary>
    /// <param name="gameDir">The game directory where the classpath file will be written.</param>
    /// <param name="mainClass">The fully-qualified main class name to launch (used when building game arguments).</param>
    /// <param name="nativesDir">The directory containing native libraries; used to replace placeholders in arguments.</param>
    /// <param name="modVersion">Optional mod loader version (e.g. forge/fabric) used when resolving placeholders.</param>
    /// <returns>
    /// A tuple containing:
    /// - Item1: the JVM argument string (may be null if placeholder replacement failed),
    /// - Item2: the game argument string (may be null if placeholder replacement failed).
    /// </returns>
    private (string?, string?) BuildArguments(string gameDir, string mainClass, string nativesDir, string? modVersion = null)
    {
        string classpath;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        // It is more readable this way
        if (OSHelper.GetOperatingSystem() == EOperatingSystem.Windows)
            classpath = string.Join(";", _classPath).Replace(@"\", @"\\");
        else
            classpath = string.Join(":", _classPath);

        _classPathFilePath = Path.Combine(gameDir, "classpath.txt");
        File.WriteAllText(_classPathFilePath, classpath);

        string? jvmArgumentString = ReplacePlaceholders(string.Join(' ',BuildJvmArguments(gameDir)), gameDir, nativesDir, modVersion);
        // TODO: Better way to add launchWrapper
        jvmArgumentString += " net.minecraft.client.main.Launch";
        
        string? gameArgumentString = ReplacePlaceholders(string.Join(' ', BuildGameArguments(mainClass)), gameDir, nativesDir, modVersion);
        return (jvmArgumentString, gameArgumentString);
    }

    /// <summary>
    /// Builds the JVM arguments for the Minecraft process.
    /// </summary>
    /// <param name="gameDir">The game directory.</param>
    /// <returns>A collection of JVM arguments.</returns>
    private IEnumerable<string> BuildJvmArguments(string gameDir)
    {
        if (MinecraftVersionMeta == null)
        {
            _logger.Error("GameDetails or MinecraftVersionMeta is null. Cannot build JVM arguments.");
            return [];
        }
        
        var jvmArgs = new List<string>
        {
            GameDetails.MinMemory > GameDetails.MaxMemory
                ? $"-Xms{GameDetails.MaxMemory}M"
                : $"-Xms{(GameDetails.MinMemory > 0 ? GameDetails.MinMemory : 256)}M",
            $"-Xmx{(GameDetails.MaxMemory > 0 ? GameDetails.MaxMemory : 2048)}M",
            $"-Dminecraft.applet.TargetDirectory=\"{gameDir}\""
        };

        if (!string.IsNullOrEmpty(GameDetails.JvmArgs))
            jvmArgs.Add(GameDetails.JvmArgs);
        
        jvmArgs.Add("-Dminecraft.api.env=custom");
        jvmArgs.Add($"-Dminecraft.api.auth.host={MesterMcEndpoints.YggdrasilEndpoint}");
        jvmArgs.Add($"-Dminecraft.api.account.host={MesterMcEndpoints.YggdrasilEndpoint}");
        jvmArgs.Add($"-Dminecraft.api.session.host={MesterMcEndpoints.YggdrasilEndpoint}");
        jvmArgs.Add($"-Dminecraft.api.services.host={MesterMcEndpoints.YggdrasilEndpoint}");
        
        var argsToAdd = _jvmArgumentsBeforeClassPath.OrderByDescending(x => x.Priority).Select(a => a.Arg);
        foreach (var arg in argsToAdd)
            jvmArgs.Add(arg);
        
        argsToAdd = MinecraftVersionMeta.GetJvmArguments();
        foreach (var arg in argsToAdd)
            jvmArgs.Add(arg);
        

        argsToAdd = _jvmArguments.OrderByDescending(x => x.Priority).Select(a => a.Arg);
        foreach (var arg in argsToAdd)
            jvmArgs.Add(arg);
        
        // Classpath fallback
        if (!jvmArgs.Any(x => x.Contains("-cp")))
            jvmArgs.Add("-cp ${classpath}");
        return jvmArgs;
    }

    /// <summary>
    /// Builds the game arguments for the Minecraft process.
    /// </summary>
    /// <param name="mainClass">The main class to launch.</param>
    /// <returns>A collection of game arguments.</returns>
    private IEnumerable<string> BuildGameArguments(string mainClass)
    {
        if (MinecraftVersionMeta == null)
        {
            _logger.Error("MinecraftVersionMeta is null. Cannot build game arguments.");
            return [];
        }
        
        var gameArgs = new List<string>
        {
            mainClass,
            MinecraftVersionMeta.GetGameArgumentString()
        };
        var gameArguments = _gameArguments.OrderByDescending(x => x.Priority).Select(a => a.Arg);
        foreach (var arg in gameArguments)
        {
            if (gameArgs.Contains(arg))
                continue;
            
            gameArgs.Add(arg);
        }

        if (Resolution is { X: > 0 })
            gameArgs.Add($"--width {Resolution.X}");
        if (Resolution is { Y: > 0 })
            gameArgs.Add($"--height {Resolution.Y}");

        if (!string.IsNullOrEmpty(GameDetails.ServerAddressToJoin))
            gameArgs.Add($"--quickPlayMultiplayer {GameDetails.ServerAddressToJoin}");

        return gameArgs;
    }

    /// <summary>
    /// Gets the version name based on the game kind and mod version.
    /// </summary>
    /// <param name="modVersion">Optional mod version string.</param>
    /// <returns>The version name as a string.</returns>
    private string GetVersionName(string? modVersion)
    {
        return GameDetails.Kind switch
        {
            EMinecraftKind.VANILLA => VersionData.MinecraftVersion,
            EMinecraftKind.FABRIC => $"fabric-loader-{modVersion}-{VersionData.MinecraftVersion}",
            EMinecraftKind.QUILT => $"quilt-loader-{modVersion}-{VersionData.MinecraftVersion}",
            EMinecraftKind.FORGE => $"{VersionData.MinecraftVersion}-forge-{modVersion}",
            EMinecraftKind.NEOFORGE => $"{VersionData.MinecraftVersion}-neoforge-{modVersion}",
            _ => VersionData.MinecraftVersion
        };
    }

    /// <summary>
    /// Replaces placeholders in the argument string with actual values.
    /// </summary>
    /// <param name="argumentString">The argument string containing placeholders.</param>
    /// <param name="gameDir">The game directory.</param>
    /// <param name="nativesDir">The directory containing native libraries.</param>
    /// <param name="modVersion">Optional mod version string.</param>
    /// <returns>The argument string with placeholders replaced.</returns>
    private string? ReplacePlaceholders(string argumentString, string gameDir, string nativesDir, string? modVersion)
    {
        if (!_isInitialized)
        {
            _logger.Error("Attempted to replace placeholders before successful initialization.");
            return null;
        }
        
        if (MinecraftVersionMeta == null)
        {
            _logger.Error("MinecraftVersionMeta is null. Cannot replace placeholders.");
            return null;
        }
        
        string gameAssetsDir = Path.Combine(PathDetails.AssetsDir, "virtual", "legacy");
        gameAssetsDir = gameAssetsDir.StartsWith('"') ? gameAssetsDir : $"\"{gameAssetsDir}\"";
        string userType = _client.IsOffline ? "offline" : "msa";
        
        var replacements = new Dictionary<string, string?>
        {
            { "${natives_directory}", nativesDir.StartsWith('"') ? nativesDir : $"\"{nativesDir}\"" },
            { "${launcher_name}", _launcherDetails.LauncherName },
            { "${launcher_version}", _launcherDetails.LauncherVersion },
            { "${auth_player_name}", _client.DisplayName },
            { "${version_name}", GetVersionName(modVersion) },
            { "${game_directory}", gameDir.StartsWith('"') ? gameDir : $"\"{gameDir}\"" },
            { "${game_assets}", gameAssetsDir },
            { "${assets_root}", PathDetails.AssetsDir.StartsWith('"') ? PathDetails.AssetsDir : $"\"{PathDetails.AssetsDir}\"" },
            { "${assets_index_name}", MinecraftVersionMeta.Index.Id },
            { "${auth_uuid}", _client.UUID },
            { "${auth_access_token}", string.IsNullOrEmpty(_client.AccessToken) ? "none" : _client.AccessToken },
            { "${auth_session}", string.IsNullOrEmpty(_client.AccessToken) ? "none" : _client.AccessToken},
            { "${clientid}", _client.ClientId },
            { "${auth_xuid}", _client.Xuid },
            { "${user_type}", userType },
            { "${version_type}", "release" },
            { "${classpath}", $"\"@{_classPathFilePath}\"" },
            { "${library_directory}", PathDetails.LibrariesDir.StartsWith('"') ? PathDetails.LibrariesDir : $"\"{PathDetails.LibrariesDir}\"" },
            { "${user_properties}", "{}" },
            { "${arch}", Environment.Is64BitOperatingSystem ? "64" : "32" }
        };

        return replacements.Aggregate(argumentString, (current, replacement) => current.Replace(replacement.Key, replacement.Value));
    }
}