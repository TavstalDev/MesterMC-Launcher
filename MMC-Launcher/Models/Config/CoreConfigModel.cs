using CommunityToolkit.Mvvm.ComponentModel;
using Tavstal.MesterMC.Launcher.Models.Config.DTOs;

namespace Tavstal.MesterMC.Launcher.Models.Config;

/// <summary>
/// Represents the core configuration model for the application, containing Java, performance, and window settings.
/// </summary>
public partial class CoreConfigModel : ObservableObject
{
    /// <summary>
    /// The Java configuration settings.
    /// </summary>
    [ObservableProperty] private JavaConfigModel _java;
    
    /// <summary>
    /// The performance configuration settings.
    /// </summary>
    [ObservableProperty] private PerformanceConfigModel _performance;
    
    /// <summary>
    /// The window configuration settings.
    /// </summary>
    [ObservableProperty] private WindowConfigModel _window;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfigModel"/> class with default values.
    /// </summary>
    public CoreConfigModel() { }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfigModel"/> class with the specified Java, performance, and window configurations.
    /// </summary>
    /// <param name="java">The Java configuration settings.</param>
    /// <param name="performance">The performance configuration settings.</param>
    /// <param name="window">The window configuration settings.</param>
    public CoreConfigModel(JavaConfigModel java, PerformanceConfigModel performance, WindowConfigModel window)
    {
        _java = java;
        _performance = performance;
        _window = window;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreConfigModel"/> class using a data transfer object (DTO).
    /// </summary>
    /// <param name="coreConfigDto">The DTO containing core configuration data.</param>
    public CoreConfigModel(CoreConfigDto coreConfigDto)
    {
        _java = new JavaConfigModel(
            coreConfigDto.Java.MinMemory,
            coreConfigDto.Java.MaxMemory,
            coreConfigDto.Java.PermaGen);
        _performance = new PerformanceConfigModel(
            coreConfigDto.Misc.UseCustomGlfw,
            coreConfigDto.Misc.CustomGlfwPath,
            coreConfigDto.Misc.UseCustomOpenAl,
            coreConfigDto.Misc.CustomOpenAlPath,
            coreConfigDto.Misc.EnableFeralGameMode,
            coreConfigDto.Misc.EnableMangoHud,
            coreConfigDto.Misc.UseDedicatedGpu
            );
        _window = new WindowConfigModel(
            coreConfigDto.Minecraft.StartMaximized,
            coreConfigDto.Minecraft.WindowWidth,
            coreConfigDto.Minecraft.WindowHeight);
    }
}