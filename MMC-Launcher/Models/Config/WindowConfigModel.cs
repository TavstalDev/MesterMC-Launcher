using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.MesterMC.Launcher.Models.Config;

/// <summary>
/// Represents the configuration model for the game window, including settings for 
/// starting maximized and specifying the window dimensions.
/// </summary>
public partial class WindowConfigModel : ObservableObject
{
    /// <summary>
    /// Gets or sets a value indicating whether the game should start maximized.
    /// </summary>
    [ObservableProperty] private bool _startMaximized;

    /// <summary>
    /// Gets or sets the width of the game window.
    /// </summary>
    [ObservableProperty] private uint _windowWidth;

    /// <summary>
    /// Gets or sets the height of the game window.
    /// </summary>
    [ObservableProperty] private uint _windowHeight;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowConfigModel"/> class with default values.
    /// </summary>
    public WindowConfigModel() {}
    
    /// <summary>
    /// Initializes a new instance of the <see cref="WindowConfigModel"/> class with the specified settings.
    /// </summary>
    /// <param name="startMaximized">Indicates whether the game should start maximized.</param>
    /// <param name="windowWidth">The width of the game window.</param>
    /// <param name="windowHeight">The height of the game window.</param>
    public WindowConfigModel(bool startMaximized, uint windowWidth, uint windowHeight)
    {
        _startMaximized = startMaximized;
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;
    }
}