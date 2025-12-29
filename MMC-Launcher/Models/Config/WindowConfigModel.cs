using CommunityToolkit.Mvvm.ComponentModel;

namespace Tavstal.MesterMC.Launcher.Models.Config;

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
    
    public WindowConfigModel() {}
    
    public WindowConfigModel(bool startMaximized, uint windowWidth, uint windowHeight)
    {
        _startMaximized = startMaximized;
        _windowWidth = windowWidth;
        _windowHeight = windowHeight;
    }
}