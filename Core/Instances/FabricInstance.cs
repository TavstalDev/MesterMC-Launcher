using Newtonsoft.Json;
using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.KonkordLauncher.Core.Models.Endpoints;
using Tavstal.KonkordLauncher.Core.Models.Fabric;
using Tavstal.KonkordLauncher.Core.Models.Installer;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta;
using Tavstal.KonkordLauncher.Core.Models.MojangApi.Meta.Library;

namespace Tavstal.KonkordLauncher.Core.Instances;

/// <summary>
/// Represents a Fabric instance, handling installation, configuration, and launching of Fabric-based Minecraft versions.
/// </summary>
public class FabricInstance(
    GameDetails gameDetails,
    PathDetails pathDetails,
    LauncherDetails launcherDetails,
    ClientDetails clientDetails,
    Resolution? resolution = null,
    IProgressReporter? progressReporter = null)
    : MinecraftInstance(gameDetails, pathDetails, launcherDetails, clientDetails, resolution, progressReporter)
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(FabricInstance));

    /// <summary>
    /// Installs the Fabric modded environment asynchronously.
    /// </summary>
    /// <param name="tempDir">The temporary directory used during installation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the modded data if successful, or null if an error occurs.</returns>
    protected override async Task<ModdedData?> InstallModdedAsync(string tempDir)
    {
        // Download version json
        FabricVersionMeta? fabricVersionMeta;
        List<LibraryMeta> localLibraries = new List<LibraryMeta>();
        string fabricJsonHash = FileSystemHelper.GetContentHash($"{GameDetails.MinecraftVersion}-fabric-{GameDetails.CustomVersion}_json")!;
        string parentDir =  Path.Combine(PathDetails.AssetsDir, "objects", fabricJsonHash[..2]);
        Directory.CreateDirectory(parentDir);
        VersionData.CustomVersionJsonPath = Path.Combine(parentDir, fabricJsonHash);

        _logger.Debug("Checking for Fabric version JSON at path: " + VersionData.CustomVersionJsonPath);
        if (!File.Exists(VersionData.CustomVersionJsonPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.SetProgress(e);
                _progressReporter?.SetStatus("A {0} verzió json fájl letöltése...", "fabric", e.ToString("0.00"));
            };

            string fabricVersionJsonUrl = string.Format(FabricEndpoints.LoaderJsonUrl, GameDetails.MinecraftVersion,
               GameDetails.CustomVersion);

            var resultJson = await HttpHelper.GetStringAsync(fabricVersionJsonUrl, progress);
            if (resultJson == null)
                return null;
                
            await File.WriteAllTextAsync(VersionData.CustomVersionJsonPath, resultJson);

            // Add the libraries
            _progressReporter?.SetStatus("A fabric verzió json fájl feldolgozása...");
            fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(resultJson);
            if (fabricVersionMeta == null)
            {
                FileSystemHelper.DeleteFile(VersionData.CustomVersionJsonPath, _progressReporter); // Delete it because this if part won't be executed again if it exists
                _logger.Error("Fabric version meta is null after deserialization. Invalid JSON format.");
                return null;
            }
        }
        else
        {
            _progressReporter?.SetStatus("A fabric verzió json fájl feldolgozása...");
            fabricVersionMeta = JsonConvert.DeserializeObject<FabricVersionMeta>(await File.ReadAllTextAsync(VersionData.CustomVersionJsonPath));
            if (fabricVersionMeta == null)
            {
                _logger.Error("Fabric version meta is null after deserialization. Invalid JSON format.");
                return null;
            }

            foreach (var lib in fabricVersionMeta.Libraries)
                localLibraries.Add(new LibraryMeta(lib.Name, new LibraryDownloads(new Artifact(lib.GetPath(), lib.Sha1, lib.Size, lib.GetURL()), null), []));
        }


        // Download Loader
        string loaderDirPath = Path.Combine(PathDetails.LibrariesDir, "net", "fabricmc", "fabric-loader", VersionData.CustomVersion);
        string loaderJarPath = Path.Combine(loaderDirPath, $"fabric-loader-{VersionData.CustomVersion}.jar");
        if (!Directory.Exists(loaderDirPath))
            Directory.CreateDirectory(loaderDirPath);

        if (!File.Exists(loaderJarPath))
        {
            Progress<double> progress = new Progress<double>();
            progress.ProgressChanged += (_, e) =>
            {
                _progressReporter?.SetProgress(e);
                _progressReporter?.SetStatus("A {0} loader letöltése...", "fabric", e.ToString("0.00"));
            };
            _logger.Debug("Downloading fabric loader jar...");
            await HttpHelper.DownloadFileAsync(string.Format(FabricEndpoints.LoaderJarUrl, VersionData.CustomVersion), loaderJarPath, progress);
        }

        ModdedData moddedData = new ModdedData(fabricVersionMeta.MainClass, VersionData, localLibraries);

        foreach (var arg in fabricVersionMeta.Arguments.GetGameArgs())
        {
            if (_gameArguments.Any(x => x.Arg == arg))
                continue;
            _gameArguments.Add(new LaunchArg(arg, 1));
        }
        
        foreach (var arg in fabricVersionMeta.Arguments.GetJvmArgs())
        {
            if (_jvmArguments.Any(x => x.Arg == arg))
                continue;

            // Fixes -DFabricMcEmu arg, without this Fabric does not load and instead the vanilla client will launch
            if (arg.StartsWith("-DFabricMcEmu="))
            {
                _jvmArguments.Add(new LaunchArg("-DFabricMcEmu=\"net.minecraft.client.main.Main\"", 1));
                continue;
            }

            _jvmArguments.Add(new LaunchArg(arg, 1));
        }
        return moddedData;
    }
}