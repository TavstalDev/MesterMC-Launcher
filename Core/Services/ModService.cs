using Tavstal.KonkordLauncher.Core.Helpers;
using Tavstal.KonkordLauncher.Core.Models;

namespace Tavstal.KonkordLauncher.Core.Services;

/// <summary>
/// Provides services for managing and verifying mods.
/// </summary>
public static class ModService
{
    // Logger instance for the ModService class.
    private static readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(ModService));
    
    /// <summary>
    /// A dictionary containing the list of mods and their corresponding SHA-256 hashes.
    /// </summary>
    public static readonly Dictionary<string, string> Mods = new() 
    {
        { "sodium-fabric-0.6.13+mc1.21.5.jar", "38db626f6286e8773b3bfcdff5ad32965ef0f0098751c63acf82bda6ac051d35" },
        { "sodium-extra-fabric-0.6.3+mc1.21.5.jar", "96f831e07203d643cda7600a911e1da63fafba9188916fb851a63804f9313e1a" },
        { "modmenu-14.0.0.jar", "19f91607ff8ee298119055c13ff3c8dbeb7513b635e030cc56e7e9b6099a8939" },
        { "lithium-fabric-0.16.3+mc1.21.5.jar", "6661b5a50fbc85c60328c68bab7361c3af6685b482f3b324dc10dbbdcf64e06d" },
        { "appleskin-fabric-mc1.21.5-3.0.6.jar", "e6fe03339204f887e295701998df4b116f92ea183394156467670ab1aaf6efb3" },
        { "reeses-sodium-options-fabric-1.8.3+mc1.21.4.jar", "b42fc0424050f076c8e1f77b7549e48ec6e07605fbf868ed5d0ef47848665dd2" },
        { "Jade-1.21.5-Fabric-18.2.0.jar", "f65529ed635ade21bcd737731afdfc7fb6086c9bea5eaa9ee601042389332568" },
        { "modelfix-1.21.5-1.12-fabric.jar", "a04945f556e3a2edbbf0306ca6f2c1e36826d2f15948d8db73b00735cdc6f33c" },
        { "fast-ip-ping-v1.0.7-mc1.21.5-fabric.jar", "e4feee50228fb45442dc36b76ce7eb9be97913c5ccb7b1e3e29a16d41c6ecc63" },
        { "PickUpNotifier-v21.5.1-1.21.5-Fabric.jar", "267f09534120de5be1b979f99cd51b1a318fb47a7d145aa095503f3324e529a4" },
        { "Controlling-fabric-1.21.5-23.0.2.jar", "532e7ab4f0d657bfc4417ac0c1c7b06a7e79d286a09fa95b3d043f1ef2528752" },
        { "voicechat-fabric-1.21.5-2.6.6.jar", "8e134c8377590c405ca9b5760fcc21104b3fa2f6eed3867645e0ee99e8ee9d99" },
        { "litematica-fabric-1.21.5-0.22.2.jar", "48c4def9c296691191527a639fd4864ad4af58a72f6a0c5d6509ef6ec2e95fc8" },
        { "chatanimation-1.0.6.jar", "5269c1cdd7ef8bd04821e400120188caa385979ab4b4932af197a8a4f17b5c22" },
        { "CraftPresence-2.7.0+1.21.5-fabric.jar", "03bc9e1bbae3150e57f01cdd51344c51b5fa3285986e8188daf4349feee21f81" },
        { "status-effect-bars-1.0.8.jar", "0b504e0cc6b5cab7116b5535bbafecae11721e1dd5f999e7c50f1c65e32d5856" },
        { "fastboot-1.21.x-v1.3.jar", "3c5963545cf51f646b4ce29c575dcb08dc23c1bcb964b5171817f6e6fe1cc074" },
        { "dynamic-fps-3.9.5+minecraft-1.21.5-fabric.jar", "0ebff6389f639ca96285eaf7ddc952359726f9cfbe627470edfb6184636f4cbb" },
        { "iris-fabric-1.8.11+mc1.21.5.jar", "e961c1fc63493b56edaea7a4934b8f8b13a913cd95437927ea18ec1e870ef93f" },
        { "placeholder-api-2.6.4+1.21.5.jar", "53249581a47cff42b6c06f3e13115e42b8d57357fc2ee89e2de4a0d849a3e22e" },
        { "OverflowingBars-v21.5.0-1.21.5-Fabric.jar", "e9951da7858b0b6e7b4afd2113ee4db8a48787ed19adda67887ca04b0cbd0a50" },
        { "fabric-api-0.128.2+1.21.5.jar", "a82fd00827206e911936ed1e0ceaec6eb55d061ca5d3c5d63c7f0031426d29ae" },
        { "Searchables-fabric-1.21.5-1.0.3.jar", "d07957d3a752d71e4ae142c8a8f332aa019d846d868746e8673e5f6bae097d5f" },
        { "yet_another_config_lib_v3-3.8.0+1.21.5-fabric.jar", "93a10d36d8d005aa5dc0cef4b8e3e33ae9ce6c3fdfa953cb028dec93565625ce" },
        { "malilib-fabric-1.21.5-0.24.2.jar", "cf248d84b0ea8a0c7af4c875e20290ceb7e592f4291546dc4703e2f630f57288" },
        { "ForgeConfigAPIPort-v21.5.3-1.21.5-Fabric.jar", "aba7a5fb52581aae164462c93a53fa4a5825838bae468e1d8393803c2b61c4dd" },
        { "PuzzlesLib-v21.5.13-1.21.5-Fabric.jar", "8e82c8c821e74fa1b8cfd1d815e38a2d217671b3ce2d811eaf962278de022d51" },
        { "UniLib-1.2.0+1.21.5-fabric.jar", "3cb887173f1dc896554bff1bdb9def83e186957ac5b7c73cc2c5b31f9cba4d91" },
        { "ok_zoomer-fabric-13.0.0-beta.3.jar", "5496a77bc2e8626638db88364947081caec8bfd8bf72f4ddb8fc49396cb0adc8" },
        { "replaymod-1.21.5-2.6.23.jar", "f6f85ee29aeaece861e221381327b2aa8d6765f33b386127ec478784fbaf7ca3" },
        { "continuity-3.0.1-test.4+1.21.5.jar", "0a2bf3d121378856ef4fadafa162a5417d09ad9b0f944ea6fbb55e0017f8811c" },
        { "CustomSkinLoader_Fabric-14.26.1-SNAPSHOT-00.jar", "5e01d067f5a6ee2fdddeb68e1ea9b6e19a72b43652ca400c62a7db20c4b437f2" },
        { "MMCAuth-1.0-SNAPSHOT.jar", "4f718ca7f2f50c174a6001935b06552c753bcff9b89d886c3673785545f90ea4"}
    };

    /// <summary>
    /// Verifies the mods in the specified directory by checking their file names and hashes.
    /// Deletes any unknown or invalid mod files.
    /// </summary>
    /// <param name="modsDirectory">The directory containing the mod files to verify.</param>
    public static void VerifyMods(string modsDirectory)
    {
        if (!Directory.Exists(modsDirectory))
            return;
        
        foreach (var file in Directory.GetFiles(modsDirectory))
        {
            string fileName = Path.GetFileName(file);
            if (!(fileName.EndsWith(".jar") || fileName.EndsWith(".zip")))
                continue;
                
            if (!Mods.TryGetValue(fileName, out var modHash))
            {
                _logger.Info("Deleting unknown mod file: " + fileName);
                File.Delete(file);
                continue;
            }

            if (!FileSystemHelper.CheckSHA256(file, modHash))
            {
                _logger.Info("Deleting invalid mod file: " + fileName);
                File.Delete(file);
                continue;
            }
        }
    }
}