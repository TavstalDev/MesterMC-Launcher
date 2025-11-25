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
    private static readonly int MaxParallelDownloads = 8;
    
    /// <summary>
    /// A dictionary containing the list of mods and their corresponding SHA-256 hashes.
    /// </summary>
    public static readonly HashSet<ModData> Mods =
    [
        new("animatica-0.6.1+1.21.5.jar", "6091b255b7c4fe7d02aa1e20373a8a5d08b23af3bfcd81c75d7dce6678a804e9", "https://cdn.modrinth.com/data/PRN43VSY/versions/CVlwSVpU/animatica-0.6.1%2B1.21.5.jar"),
        new("appleskin-fabric-mc1.21.6-3.0.6.jar", "b82abe586851ba6dbb9bcd4d9270fbddcd519e855ea7bffe933ed45ab5158e21", "https://cdn.modrinth.com/data/EsAfCjCV/versions/YAjCkZ29/appleskin-fabric-mc1.21.6-3.0.6.jar"),
        new("bettermounthud-1.2.6.jar", "98a625d51db9ee1d2548bef0fc28540e991034654da14ceb2b541da4fc695402", "https://cdn.modrinth.com/data/kqJFAPU9/versions/rXZxHSEZ/bettermounthud-1.2.6.jar"),
        new("BetterGrassify-1.8.2+fabric.1.21.10.jar", "94c30e62a1bd2fd5710cac4b9348f484ec8e6127705910ca6b45b46ae061b15d", "https://cdn.modrinth.com/data/m5T5xmUy/versions/2C7y66BK/BetterGrassify-1.8.2%2Bfabric.1.21.10.jar"),
        new("chatanimation-1.0.7.jar", "524b8bfdcdb567e109ddd80ebd9e6c66e0cf6455cb869a9b440023dbdaa80963", "https://cdn.modrinth.com/data/DnNYdJsx/versions/UFrXjD4k/chatanimation-1.0.7.jar"),
        new("cloth-config-19.0.147-fabric.jar", "d8a6dca9d0dad1fe44622a3c6a0348641934e3943c8d425a26f112bce6debfa2", "https://cdn.modrinth.com/data/9s6osm5g/versions/cz0b1j8R/cloth-config-19.0.147-fabric.jar"),
        new("continuity-3.0.1-beta.1+1.21.6.jar", "80b03195bbfdc805a3280d6b6354e3ecdeb6a3c936fd2928a71fac8bee619708", "https://cdn.modrinth.com/data/1IjD5062/versions/m0cvWhzT/continuity-3.0.1-beta.1%2B1.21.6.jar"),
        new("CraftPresence-2.7.0+1.21.8-fabric.jar", "a08c55ac8cec21cee3bbfc1f939b374b31eaa084aa3ff2308be8f8fdc43a1738", "https://cdn.modrinth.com/data/DFqQfIBR/versions/4vj0xtr0/CraftPresence-2.7.0%2B1.21.8-fabric.jar"),
        new("cwb-fabric-3.0.0+mc1.21.5.jar", "0ffe5494c49758a5c3ba407aa25cb33962f7d1b00d1068cfbadf5104a219766a", "https://cdn.modrinth.com/data/ETlrkaYF/versions/wXhtL4fb/cwb-fabric-3.0.0%2Bmc1.21.5.jar"),
        new("CustomSkinLoader_Fabric-14.26.1.jar", "733af00a53a9d66719f09f5cb3d630c4bb22ce043b2c66700a234c578f65c621", "https://cdn.modrinth.com/data/idMHQ4n2/versions/bLZg6wUJ/CustomSkinLoader_Fabric-14.26.1.jar"),
        new("debugify-1.21.8+1.0.jar", "5277a32bb4c21fbe0752bea28e90e6c365da82c6d6a0a7b26732f2a0ebd3a775", "https://cdn.modrinth.com/data/QwxR6Gcd/versions/WLSwJeXa/debugify-1.21.8%2B1.0.jar"),
        new("dynamic-fps-3.9.6+minecraft-1.21.6-fabric.jar", "bc1173b967b23368138ee8539ab12c7865339a7696486bfad61dd7062d33c4a2", "https://cdn.modrinth.com/data/LQ3K71Q1/versions/PqIDU2GY/dynamic-fps-3.9.6%2Bminecraft-1.21.6-fabric.jar"),
        new("e4mc_minecraft-fabric-5.4.1.jar", "42d722bceb020190509d1c228d3bd9714cf9090903ba80ad2fd525fe830e70bf", "https://cdn.modrinth.com/data/qANg5Jrr/versions/baNcxaPZ/e4mc_minecraft-fabric-5.4.1.jar"),
        new("entity_model_features_1.21.6-fabric-3.0.1.jar", "3603c541021cebc538fe449632acd704bf01e2e6e86385f22ef896a0c0e92ed8", "https://cdn.modrinth.com/data/4I1XuqiY/versions/PHCCbdMs/entity_model_features_1.21.6-fabric-3.0.1.jar"),
        new("entity_texture_features_1.21.6-fabric-7.0.2.jar", "deb60880ede708e3bbd3c20bfd668ce6bdcc070422248311b433dd3dc91e67d8", "https://cdn.modrinth.com/data/BVzZfTc1/versions/ZGrSwKTR/entity_texture_features_1.21.6-fabric-7.0.2.jar"),
        new("entityculling-fabric-1.9.3-mc1.21.8.jar", "bf64ca808bb8b06b7120c2efb2fea2fc783072ec40290ca7b146c1b2545e4a25", "https://cdn.modrinth.com/data/NNAgCjsB/versions/U81jswDa/entityculling-fabric-1.9.3-mc1.21.8.jar"),
        new("fabric-api-0.136.0+1.21.8.jar", "1036704991b3efbf8a3d03ffd38355274cb00202ae45c768a8f630c194d12ed6", "https://cdn.modrinth.com/data/P7dR8mSH/versions/RMahJx2I/fabric-api-0.136.0%2B1.21.8.jar"),
        new("fabric-language-kotlin-1.13.7+kotlin.2.2.21.jar", "77951963edd5d7d37ee4f174e37d46a871e45b973426f3b49d6725c819b4b8f2", "https://cdn.modrinth.com/data/Ha28R6CL/versions/LcgnDDmT/fabric-language-kotlin-1.13.7%2Bkotlin.2.2.21.jar"),
        new("fabrishot-1.16.2.jar", "8296195b1e39cff6c3e80bd12ba332a2237a2111cd65a7c0110800d553be9d46", "https://cdn.modrinth.com/data/3qsfQtE9/versions/qaV4jqYg/fabrishot-1.16.2.jar"),
        new("fastquit-3.1.1+mc1.21.6.jar", "d9d0d39b010a5411e454ae75c2080fbb4c910448b1e4a88922e1c6c1446cabab", "https://cdn.modrinth.com/data/x1hIzbuY/versions/ah71vPRw/fastquit-3.1.1%2Bmc1.21.6.jar"),
        new("ferritecore-8.0.0-fabric.jar", "2b90bf00c2a5808c3c539712a55691191f8716d5bfa6eefaba35e9c4c5a28eea", "https://cdn.modrinth.com/data/uXXizFIs/versions/CtMpt7Jr/ferritecore-8.0.0-fabric.jar"),
        new("ForgeConfigAPIPort-v21.8.1-1.21.8-Fabric.jar", "272fddcf3cc81d557211ec5cd3b94608ae2f4e695987fb69c9911bdbbb6ff0fc", "https://cdn.modrinth.com/data/ohNO6lps/versions/daREdLQt/ForgeConfigAPIPort-v21.8.1-1.21.8-Fabric.jar"),
        new("ImmediatelyFast-Fabric-1.12.2+1.21.8.jar", "e6934e2e0028801eac5c6809853917e6fc350306fc49c7a56bfc38fb5813ba6d", "https://cdn.modrinth.com/data/5ZwdcRci/versions/OrO3H19n/ImmediatelyFast-Fabric-1.12.2%2B1.21.8.jar"),
        new("iris-fabric-1.9.6+mc1.21.8.jar", "804f0cdf2d6a06baf5cd5b50c1c9cc1ec187bd3d394da4fee15c70d2b8dcccd0", "https://cdn.modrinth.com/data/YL57xq9U/versions/Rhzf61g1/iris-fabric-1.9.6%2Bmc1.21.8.jar"),
        new("language-reload-1.7.4+1.21.6.jar", "1126d67e96acdb66f0684f37da869c98dcbcacdea686782370c650e24cf7e39d", "https://cdn.modrinth.com/data/uLbm7CG6/versions/W8KDnevt/language-reload-1.7.4%2B1.21.6.jar"),
        new("lithium-fabric-0.18.1+mc1.21.8.jar", "04f370c2f6e819dcd86a3c5684e38fd1e9c4988c7c2493ffb2bc07eb4504b94c", "https://cdn.modrinth.com/data/gvQqBUqZ/versions/qxIL7Kb8/lithium-fabric-0.18.1%2Bmc1.21.8.jar"),
        new("mixintrace-1.1.1+1.17.jar", "26ca21a27706cfa4561868f31c4fd07c542e8cc759419f5884ddff1f3a126a99", "https://cdn.modrinth.com/data/sGmHWmeL/versions/1.1.1%2B1.17/mixintrace-1.1.1%2B1.17.jar"),
        new("modmenu-15.0.0.jar", "5a6459c2760e35a0086c813fafe3fe61a964a5fddd94a331b2284a502ba1792f", "https://cdn.modrinth.com/data/mOgUt4GM/versions/am1Siv7F/modmenu-15.0.0.jar"),
        new("modelfix-1.21.5-1.12-fabric.jar", "a04945f556e3a2edbbf0306ca6f2c1e36826d2f15948d8db73b00735cdc6f33c", "https://cdn.modrinth.com/data/QdG47OkI/versions/WcDxGReS/modelfix-1.21.5-1.12-fabric.jar"),
        new("moreculling-fabric-1.21.8-1.4.0-beta.2.jar", "deffc4b9c50b9ec8439a61eb3ab3128a279dbffd1dcd0ad31b893d92183ef600", "https://cdn.modrinth.com/data/51shyZVL/versions/ivOsScf8/moreculling-fabric-1.21.8-1.4.0-beta.2.jar"),
        new("NoChatReports-FABRIC-1.21.8-v2.15.0.jar", "f21bf1c79fadb1277c756abc7cf07c0edd7ce954706cf7ddf9fcc1b0ee176c90", "https://cdn.modrinth.com/data/qQyHxfxd/versions/pmpg6ocz/NoChatReports-FABRIC-1.21.8-v2.15.0.jar"),
        new("optiboxes-1.7+1.21.8-fabric.jar", "00d9c5dd6b76313f76686f332818e2c2281255cb70da0e3273860d79384bbef0", "https://cdn.modrinth.com/data/DWuwk8aA/versions/VHppll0O/optiboxes-1.7%2B1.21.8-fabric.jar"),
        new("optigui-2.3.0-beta.8+1.21.6.jar", "7e95944434be0c11347b110ab399609a567e33a5751905b08f8bebd65bc66db7", "https://cdn.modrinth.com/data/JuksLGBQ/versions/ft3Pi0Dc/optigui-2.3.0-beta.8%2B1.21.6.jar"),
        new("paginatedadvancements-2.7.0+1.21.8.jar", "c82248aad622352c2eb0d8b7e2fd4505213f8039d0fac2984e1a57caf876c21c", "https://cdn.modrinth.com/data/pJogNFap/versions/yErEOfqA/paginatedadvancements-2.7.0%2B1.21.8.jar"),
        new("PickUpNotifier-v21.8.1-1.21.8-Fabric.jar", "72fe4e8008ea4217e488ac6f721c8ce7ca09917952bd5f423c32a62367d2fad3", "https://cdn.modrinth.com/data/ZX66K16c/versions/CQxBBsyi/PickUpNotifier-v21.8.1-1.21.8-Fabric.jar"),
        new("puzzle-fabric-2.1.1+1.21.6.jar", "601ead8a8b27319787d9094a5a690d7a79e2297da100cabeab692da8fe19ccfe", "https://cdn.modrinth.com/data/3IuO68q1/versions/EfTbdnT6/puzzle-fabric-2.1.1%2B1.21.6.jar"),
        new("PuzzlesLib-v21.8.9-1.21.8-Fabric.jar", "f9ab44080a0756876e42e777bbe111dee2e5b87c75eea9425a7b7b0f91dbda61", "https://cdn.modrinth.com/data/QAGBst4M/versions/tXTEdgyF/PuzzlesLib-v21.8.9-1.21.8-Fabric.jar"),
        new("reeses-sodium-options-fabric-1.8.4+mc1.21.6.jar", "bc140a2af0a3cf7dcb691be5d88263d9db87468d643a666ad6f881ddcb23482a", "https://cdn.modrinth.com/data/Bh37bMuy/versions/AgGRyydH/reeses-sodium-options-fabric-1.8.4%2Bmc1.21.6.jar"),
        new("rrls-5.1.10+mc1.21.8-fabric.jar", "92f6e26d6aba4b9a74097f90cdf758a7994f0dd39896b3993e070ef292fea07b", "https://cdn.modrinth.com/data/ZP7xHXtw/versions/hkfSWmNV/rrls-5.1.10%2Bmc1.21.8-fabric.jar"),
        new("voicechat-fabric-1.21.8-2.6.6.jar", "fefbf1bc1ab9618e93baedd46ab05799d89537484e553c0bfde357dc72e8d321", "https://cdn.modrinth.com/data/9eGKb6K1/versions/2Z1g1v36/voicechat-fabric-1.21.8-2.6.6.jar" ),
        new("sodium-fabric-0.7.3+mc1.21.8.jar", "388323a88c22a56357ff3f6b782b446ceb47006122b6bd62a9ec81b22bef2d70", "https://cdn.modrinth.com/data/AANobbMI/versions/7pwil2dy/sodium-fabric-0.7.3%2Bmc1.21.8.jar"),
        new("sodium-extra-fabric-0.7.0+mc1.21.8.jar", "853923fba108a00ef4320814d81e93db6c55d3aeb5220fe4039d3cb3f001dbe4", "https://cdn.modrinth.com/data/PtjYWJkn/versions/Of25zuEG/sodium-extra-fabric-0.7.0%2Bmc1.21.8.jar"),
        new("UniLib-1.2.0+1.21.8-fabric.jar", "efb1289469874ad43d7a2273590960dffd1e134f588e3f21291c7f8443acbcc4", "https://cdn.modrinth.com/data/nT86WUER/versions/BEkYsae8/UniLib-1.2.0%2B1.21.8-fabric.jar"),
        new("yet_another_config_lib_v3-3.7.1+1.21.6-fabric.jar", "7d71fbee0e2ca0f38a475309d23c9d084cd68edd4be6544eecdb5aec1f36d1ed", "https://cdn.modrinth.com/data/1eAoo2KR/versions/WxYlHLu6/yet_another_config_lib_v3-3.7.1%2B1.21.6-fabric.jar"),
        new("yosbr-0.1.2.jar", "db4c744fd71f5617639cb0fdff72378b08d2852004f4045c62090de1bf53afcb", "https://cdn.modrinth.com/data/WwbubTsV/versions/KMOzdYko/yosbr-0.1.2.jar"),
        new("Zoomify-2.14.6+1.21.6.jar", "f730fc2f5e2b0a5f285f9ed01a307f2c5cecb13e5527fcb6971b4b398c85549a", "https://cdn.modrinth.com/data/w7ThoJFB/versions/qMqviL3t/Zoomify-2.14.6%2B1.21.6.jar"),
    
        // Disabled mods
        new("litematica-fabric-1.21.8-0.23.5.jar", "1f04fa397b9d544286d95c5582e96a7dd9ae5a5ea3755d8e69fa34a748c406e0", "https://cdn.modrinth.com/data/bEpr0Arc/versions/XWl870Bx/litematica-fabric-1.21.8-0.23.5.jar", true),
        new("replaymod-1.21.7-2.6.23.jar", "8af03eadc6d3781593c6d37fde004c6434c2ab8e527085daac71f01c0861171f", "https://cdn.modrinth.com/data/Nv2fQJo5/versions/TUHG3lET/replaymod-1.21.7-2.6.23.jar", true),
    ];

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
                
            var modEntry = Mods.FirstOrDefault(m => m.Name == fileName);
            if (modEntry == null)
            {
                _logger.Info("Deleting unknown mod file: " + fileName);
                File.Delete(file);
                continue;
            }
            
            if (!FileSystemHelper.CheckSHA256(file, modEntry.Sha256Hash))
            {
                _logger.Info("Deleting invalid mod file: " + fileName);
                File.Delete(file);
            }
        }
    }
    
    public static async Task DownloadModsAsync(string modsDirectory, IProgress<double>? progress = null)
    {
        if (!Directory.Exists(modsDirectory))
            Directory.CreateDirectory(modsDirectory);
        
        int totalMods = Mods.Count;
        int downloadedMods = 0;
        
        var semaphore = new SemaphoreSlim(MaxParallelDownloads);
        var tasks = new List<Task>();

        foreach (var mod in Mods)
        {
            await semaphore.WaitAsync();
            var t = Task.Run(async () =>
            {
                try
                {
                    string modPath = Path.Combine(modsDirectory, mod.Name);
                    string modDisabledPath = modPath + ".dis";
                    if (File.Exists(modPath) || File.Exists(modDisabledPath) || string.IsNullOrEmpty(mod.Url))
                    {
                        Interlocked.Add(ref downloadedMods, 1);
                        progress?.Report((double)downloadedMods / totalMods * 100d);
                        return;
                    }
                    string finalPath = mod.IsDisabled ? modDisabledPath : modPath;

                    _logger.Info("Downloading " + mod.Name + " from " + mod.Url);
                    await HttpHelper.DownloadFileAsync(mod.Url, finalPath, progress);

                    if (!FileSystemHelper.CheckSHA256(finalPath, mod.Sha256Hash))
                        _logger.Error("Downloaded mod has invalid hash: " + mod.Name);

                    Interlocked.Add(ref downloadedMods, 1);
                    progress?.Report((double)downloadedMods / totalMods * 100d);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            tasks.Add(t);
        }
        
        await Task.WhenAll(tasks);
    }
}