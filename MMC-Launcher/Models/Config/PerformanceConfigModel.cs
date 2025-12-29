using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.MesterMC.Launcher.Models.Config;

public partial class PerformanceConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the command to execute before launching the application.
    /// </summary>
    [ObservableProperty] private string? _preLaunchCommand;

    /// <summary>
    /// Gets or sets the wrapper command to execute during the application's runtime.
    /// </summary>
    [ObservableProperty] private string? _wrapperCommand;

    /// <summary>
    /// Gets or sets the command to execute after the application exits.
    /// </summary>
    [ObservableProperty] private string? _postExitCommand;

    /// <summary>
    /// Gets or sets a value indicating whether a custom GLFW library should be used.
    /// </summary>
    [ObservableProperty] private bool _useCustomGlfw;

    /// <summary>
    /// Gets or sets the file path to the custom GLFW library.
    /// </summary>
    [ObservableProperty] private string? _customGlfwPath;

    /// <summary>
    /// Gets or sets a value indicating whether a custom OpenAL library should be used.
    /// </summary>
    [ObservableProperty] private bool _useCustomOpenAl;

    /// <summary>
    /// Gets or sets the file path to the custom OpenAL library.
    /// </summary>
    [ObservableProperty] private string? _customOpenAlPath;

    /// <summary>
    /// Gets or sets a value indicating whether Feral GameMode should be enabled.
    /// </summary>
    [ObservableProperty] private bool _enableFeralGameMode;

    /// <summary>
    /// Gets or sets a value indicating whether MangoHud should be enabled.
    /// </summary>
    [ObservableProperty] private bool _enableMangoHud;

    /// <summary>
    /// Gets or sets a value indicating whether a dedicated GPU should be used.
    /// </summary>
    [ObservableProperty] private bool _useDedicatedGpu;
    
    public PerformanceConfigModel() {}
    
    public PerformanceConfigModel(string? preLaunchCommand, string? wrapperCommand, string? postExitCommand, bool useCustomGlfw, string? customGlfwPath, bool useCustomOpenAl, string? customOpenAlPath, bool enableFeralGameMode, bool enableMangoHud, bool useDedicatedGpu)
    {
        _preLaunchCommand = preLaunchCommand;
        _wrapperCommand = wrapperCommand;
        _postExitCommand = postExitCommand;
        _useCustomGlfw = useCustomGlfw;
        _customGlfwPath = customGlfwPath;
        _useCustomOpenAl = useCustomOpenAl;
        _customOpenAlPath = customOpenAlPath;
        _enableFeralGameMode = enableFeralGameMode;
        _enableMangoHud = enableMangoHud;
        _useDedicatedGpu = useDedicatedGpu;
    }
}