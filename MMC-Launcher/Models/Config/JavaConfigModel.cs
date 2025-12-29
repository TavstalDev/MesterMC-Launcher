using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.MesterMC.Launcher.Models.Config;

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
    /// Gets or sets the JVM arguments to be used when launching Java.
    /// </summary>
    [ObservableProperty] private string _jvmArguments;
    
    public JavaConfigModel() {}
    
    public JavaConfigModel(uint minMemory, uint maxMemory, uint permaGen, string defaultJavaPath, string jvmArguments)
    {
        _minMemory = minMemory;
        _maxMemory = maxMemory;
        _permaGen = permaGen;
        _defaultJavaPath = defaultJavaPath;
        _jvmArguments = jvmArguments;
    }
}