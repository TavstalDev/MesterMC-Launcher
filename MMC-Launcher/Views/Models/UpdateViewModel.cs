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
    
    [ObservableProperty] private Bitmap screenImage = ImageHelper.LoadFromResource(new Uri("avares://MMC-Launcher/Assets/screens/screen-update-2.png"));
    
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
}