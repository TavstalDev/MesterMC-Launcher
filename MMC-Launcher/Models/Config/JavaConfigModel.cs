using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.MesterMC.Launcher.Models.Config;

/// <summary>
/// Represents the configuration model for Java settings, including memory allocation, 
/// Java executable path, and JVM arguments.
/// </summary>
public partial class JavaConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the minimum memory allocation for Java in megabytes.
    /// </summary>
    [ObservableProperty] private uint _minMemory;

    /// <summary>
    /// Gets or sets the maximum memory allocation for Java in megabytes.
    /// </summary>
    [ObservableProperty] private uint _maxMemory;

    /// <summary>
    /// Gets or sets the permanent generation memory size for Java in megabytes.
    /// </summary>
    [ObservableProperty] private uint _permaGen;

    /// <summary>
    /// Gets or sets the default file path to the Java executable.
    /// </summary>
    [ObservableProperty] private string _defaultJavaPath;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="JavaConfigModel"/> class with default values.
    /// </summary>
    public JavaConfigModel() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="JavaConfigModel"/> class with the specified settings.
    /// </summary>
    /// <param name="minMemory">The minimum memory allocation for Java in megabytes.</param>
    /// <param name="maxMemory">The maximum memory allocation for Java in megabytes.</param>
    /// <param name="permaGen">The permanent generation memory size for Java in megabytes.</param>
    /// <param name="defaultJavaPath">The default file path to the Java executable.</param>
    public JavaConfigModel(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath)
    {
        _minMemory = minMemory;
        _maxMemory = maxMemory;
        _permaGen = permaGen;
        _defaultJavaPath = defaultJavaPath;
    }
}