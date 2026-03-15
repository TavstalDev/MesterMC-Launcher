using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using Tavstal.MesterMC.Launcher.Helpers;

namespace Tavstal.MesterMC.Launcher.Views.Models;

/// <summary>
/// ViewModel used by the update view/screen.
/// Exposes progress state, a progress text, a screen image, and a close interaction/command for the view to bind to.
/// </summary>
public partial class UpdateViewModel : ObservableObject
{
    /// <summary>
    /// The progress value, represented as a double.
    /// </summary>
    [ObservableProperty] private double _progress;

    /// <summary>
    /// The progress text, initialized with a default value of "...".
    /// </summary>
    [ObservableProperty] private string _progressText = "...";
    
    /// <summary>
    /// Backing field for the generated ScreenImage property.
    /// Holds the image shown on the update screen.
    /// </summary>
    [ObservableProperty] private Bitmap screenImage = ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/screens/screen-update-2.png"));
    
    /// <summary>
    /// Interaction used to request the view/window to close.
    /// </summary>
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    /// Command method that triggers the close interaction.
    /// The [RelayCommand] attribute generates an ICommand the view can bind to.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
}