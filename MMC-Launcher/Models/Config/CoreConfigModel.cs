using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.KonkordLauncher.Common.Models.Config;

namespace Tavstal.MesterMC.Launcher.Models.Config;

public partial class CoreConfigModel : ObservableObject
{
    [ObservableProperty] private JavaConfigModel _java;
    
    [ObservableProperty] private PerformanceConfigModel _performance;
    
    [ObservableProperty] private WindowConfigModel _window;
    
    public CoreConfigModel() { }
    
    public CoreConfigModel(JavaConfigModel java, PerformanceConfigModel performance, WindowConfigModel window)
    {
        _java = java;
        _performance = performance;
        _window = window;
    }

    public CoreConfigModel(CoreConfig coreConfig)
    {
        _java = new JavaConfigModel(
            coreConfig.Java.MinMemory,
            coreConfig.Java.MaxMemory,
            coreConfig.Java.PermaGen,
            coreConfig.Java.JavaPath,
            coreConfig.Java.JvmArguments);
        _performance = new PerformanceConfigModel(coreConfig.Misc.PreLaunchCommand, 
            coreConfig.Misc.WrapperCommand,
            coreConfig.Misc.PostExitCommand,
            coreConfig.Misc.UseCustomGlfw,
            coreConfig.Misc.CustomGlfwPath,
            coreConfig.Misc.UseCustomOpenAl,
            coreConfig.Misc.CustomOpenAlPath,
            coreConfig.Misc.EnableFeralGameMode,
            coreConfig.Misc.EnableMangoHud,
            coreConfig.Misc.UseDedicatedGpu
            );
        _window = new WindowConfigModel(
            coreConfig.Minecraft.StartMaximized,
            coreConfig.Minecraft.WindowWidth,
            coreConfig.Minecraft.WindowHeight);
    }
}