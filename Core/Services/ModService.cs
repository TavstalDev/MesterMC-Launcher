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
        { "BetterGrassify-1.7.0+fabric.1.21.5.jar", "066a4e49671cd9a89971a162f7336c616fcf8e4eadd41dbebf92c5811cedcd65" },
        { "animatica-0.6.1+1.21.5.jar", "6091b255b7c4fe7d02aa1e20373a8a5d08b23af3bfcd81c75d7dce6678a804e9" },
        { "ImmediatelyFast-Fabric-1.9.6+1.21.5.jar", "6fe683023cb2894953f0bb00d1da977a4bb81e46b0100ed808a92fc380564ae3" },
        { "Debugify-1.21.5+1.0.jar", "a4ec1f42d2ff139ca11894456063be736b28169220bfdc7f10fe1890bb7a7156" },
        { "bettermounthud-1.2.5.jar", "4723ebac23c3a210117f2231155bc869942e6e4cdcee4b0f26c6fcf355458330" },
        { "NoChatReports-FABRIC-1.21.5-v2.12.0.jar", "d0f8f5638511c865f65df20a016a4fc149a9023842cbe50ee422183af9ca561d" },
        { "ForgeConfigAPIPort-v21.5.1-1.21.5-Fabric.jar", "3ce8d8ddd6983e51e7be21acc52871a87611a4beabe7d548cf0adc0a42e5e123" },
        { "Zoomify-2.14.2+1.21.3.jar", "f6767f5f6b42576fd4c184439e40230410616fe3519fe3896f9d6a70f8a0b5b2" },
        { "cwb-fabric-3.0.0+mc1.21.5.jar", "0ffe5494c49758a5c3ba407aa25cb33962f7d1b00d1068cfbadf5104a219766a" },
        { "dynamic-fps-3.9.5+minecraft-1.21.5-fabric.jar", "0ebff6389f639ca96285eaf7ddc952359726f9cfbe627470edfb6184636f4cbb" },
        { "continuity-3.0.1-test.4+1.21.5.jar", "0a2bf3d121378856ef4fadafa162a5417d09ad9b0f944ea6fbb55e0017f8811c" },
        { "cloth-config-18.0.145-fabric.jar", "ec67012761aee86c140a910358c49ddbc24b852e980707940705721e1bff5e7f" },
        { "entity_model_features_1.21.5-fabric-3.0.1.jar", "4bf8cdc8a9797e32123d23cb803b75f99e2085f16568fdc2eb1a17618c0e2d2f" },
        { "entity_texture_features_1.21.5-fabric-7.0.2.jar", "890d17f9caacfe5f532bc4f475cba7e9ae76f45cc6486fee35fbd917a52ab2ed" },
        { "fabrishot-1.15.1.jar", "61deed4781cea86432d949af5a8fd4f9dc27e23ad0e14bd8959741e7a1314804" },
        { "entityculling-fabric-1.8.2-mc1.21.5.jar", "62b1641bb0c3016d3f406eae79414620080b0d0870671159bee50384a04d35ea" },
        { "fastquit-3.0.0+1.21.4.jar", "bc800711a9c2fdd1614f1a88527b6514a79a5fb1fae6bafb00dd2c2188062510" },
        { "ferritecore-8.0.0-fabric.jar", "2b90bf00c2a5808c3c539712a55691191f8716d5bfa6eefaba35e9c4c5a28eea" },
        { "language-reload-1.7.4+1.21.5.jar", "b99f613b49c4de7836589bb8467a4e72fa6ca13446fa48103c92a887e9d414a9" },
        { "fabric-api-0.128.2+1.21.5.jar", "a82fd00827206e911936ed1e0ceaec6eb55d061ca5d3c5d63c7f0031426d29ae" },
        { "lithium-fabric-0.16.3+mc1.21.5.jar", "6661b5a50fbc85c60328c68bab7361c3af6685b482f3b324dc10dbbdcf64e06d" },
        { "lambdynamiclights-4.2.10+1.21.5.jar", "833655f3959765447310a623f7a88d075e30d0c303ba57652a2af08f276921a7" },
        { "mixintrace-1.1.1+1.17.jar", "26ca21a27706cfa4561868f31c4fd07c542e8cc759419f5884ddff1f3a126a99" },
        { "modelfix-1.21.5-1.12-fabric.jar", "a04945f556e3a2edbbf0306ca6f2c1e36826d2f15948d8db73b00735cdc6f33c" },
        { "morechathistory-1.3.1.jar", "f6032e46826b63e18c8848a3053e32652fc5298892a216a53dce66598925fc6c" },
        { "moreculling-fabric-1.21.5-1.3.1.jar", "d72beecb38c57ff5a5c42ed35d7458d57d508eaee19e78432a670e3bba329d09" },
        { "optiboxes-1.4+mc1.21.5-fa368c4.jar", "11aa889a84dfbe31dd8fbdb7d6096f089d9648e526f2ddea2d449088bceab8bb" },
        { "iris-fabric-1.8.11+mc1.21.5.jar", "e961c1fc63493b56edaea7a4934b8f8b13a913cd95437927ea18ec1e870ef93f" },
        { "paginatedadvancements-2.7.0+1.21.5.jar", "f3051695a24d45101d9651a9faa078ff3001f6bf94386af981c1f6f97acbff68" },
        { "modmenu-14.0.0-rc.2.jar", "312f3208349aa36920a12f45a08bd58a3d1c5cc96052656b4fdecf8e31810d69" },
        { "puzzle-fabric-2.1.0+1.21.5.jar", "0a9bc1442615ef14b8d618eac6de62407b6e1d809a30e3e2f1b58ca86af2b9a9" },
        { "reeses-sodium-options-fabric-1.8.3+mc1.21.4.jar", "b42fc0424050f076c8e1f77b7549e48ec6e07605fbf868ed5d0ef47848665dd2" },
        { "optigui-2.3.0-beta.7+1.21.5.jar", "d8b4ee2dd4dc89d00b17a5ec6848b072872d804211fede230673f07e373eed11" },
        { "rrls-5.1.6+mc1.21.5-fabric.jar", "09d27e2dbc4a9e615df647d4de45808639bde7497bfa8f4c45ef6b99cac0f7d5" },
        { "polytone-1.21.5-3.5.12-fabric.jar", "81f24ab817730d5a96194fa75117dc61a9c4d730a3dd558032fb7281128f6653" },
        { "sodium-extra-fabric-0.6.3+mc1.21.5.jar", "96f831e07203d643cda7600a911e1da63fafba9188916fb851a63804f9313e1a" },
        { "yosbr-0.1.2.jar", "db4c744fd71f5617639cb0fdff72378b08d2852004f4045c62090de1bf53afcb" },
        { "sodium-fabric-0.6.13+mc1.21.5.jar", "38db626f6286e8773b3bfcdff5ad32965ef0f0098751c63acf82bda6ac051d35" },
        { "yet_another_config_lib_v3-3.7.1+1.21.5-fabric.jar", "56dd3d9906a9df0227b904f472b993595b0587dfa6731f8af9ce2139ea4927a0" },
        { "fabric-language-kotlin-1.13.5+kotlin.2.2.10.jar", "c1a8fbb4e4ef6a7211cdef70431795a4e6d453fe5713fb12c1e6279d64aaad84" },
        { "Controlling-fabric-1.21.5-23.0.2.jar", "532e7ab4f0d657bfc4417ac0c1c7b06a7e79d286a09fa95b3d043f1ef2528752" },
        { "appleskin-fabric-mc1.21.5-3.0.6.jar", "e6fe03339204f887e295701998df4b116f92ea183394156467670ab1aaf6efb3" },
        { "BetterPingDisplay-Fabric-1.21.5-1.1.1.jar", "66ac36e20c30a3913fec1b9f8ab215e4c4be02d1c2831451711f5560e0167234" },
        { "Jade-1.21.5-Fabric-18.2.0.jar", "f65529ed635ade21bcd737731afdfc7fb6086c9bea5eaa9ee601042389332568" },
        { "PickUpNotifier-v21.5.1-1.21.5-Fabric.jar", "267f09534120de5be1b979f99cd51b1a318fb47a7d145aa095503f3324e529a4" },
        { "chatanimation-1.0.6.jar", "5269c1cdd7ef8bd04821e400120188caa385979ab4b4932af197a8a4f17b5c22" },
        { "litematica-fabric-1.21.5-0.22.2.jar", "48c4def9c296691191527a639fd4864ad4af58a72f6a0c5d6509ef6ec2e95fc8" },
        { "Searchables-fabric-1.21.5-1.0.3.jar", "d07957d3a752d71e4ae142c8a8f332aa019d846d868746e8673e5f6bae097d5f" },
        { "RoughlyEnoughItems-19.0.809-fabric.jar", "d28b88191f22f63ad552b04c498454a39724a701cfc7814b054adc7982facf23" },
        { "architectury-16.1.4-fabric.jar", "a9d53c7c4f6aa3329c27f2b882b9cae9e38706fd6b61cc922e4b8529e8ab031b" },
        { "CraftPresence-2.7.0+1.21.5-fabric.jar", "03bc9e1bbae3150e57f01cdd51344c51b5fa3285986e8188daf4349feee21f81" },
        { "malilib-fabric-1.21.5-0.24.2.jar", "cf248d84b0ea8a0c7af4c875e20290ceb7e592f4291546dc4703e2f630f57288" },
        { "PuzzlesLib-v21.5.13-1.21.5-Fabric.jar", "8e82c8c821e74fa1b8cfd1d815e38a2d217671b3ce2d811eaf962278de022d51" },
        { "voicechat-fabric-1.21.5-2.6.6.jar", "8e134c8377590c405ca9b5760fcc21104b3fa2f6eed3867645e0ee99e8ee9d99" },
        { "UniLib-1.2.0+1.21.5-fabric.jar", "3cb887173f1dc896554bff1bdb9def83e186957ac5b7c73cc2c5b31f9cba4d91" },
        { "replaymod-1.21.5-2.6.23.jar", "f6f85ee29aeaece861e221381327b2aa8d6765f33b386127ec478784fbaf7ca3" },
        { "WorldEditCUI-1.21.5+01.jar", "350078ea08c302d603bb443031d141f1022e312aac1746683fb2dd97b15a50bc" },
        { "CustomSkinLoader_Fabric-14.26.1.jar", "733af00a53a9d66719f09f5cb3d630c4bb22ce043b2c66700a234c578f65c621" },
        { "MMCAuth-1.0-SNAPSHOT.jar", "5c5602d2238c5a6be717b7ab5693dd39f49587fa035f7157803d25a754d36ff4"}
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