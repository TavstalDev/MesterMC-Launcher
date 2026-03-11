using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace Tavstal.MesterMC.Updater.Views.Models;

/// <summary>
/// Represents the view model for the update process in the MMC-Updater application.
/// Inherits from <see cref="ObservableObject"/> to provide property change notifications.
/// </summary>
public partial class UpdateViewModel : ObservableObject
{
    /// <summary>
    /// Gets or sets the progress value of the update process.
    /// This property is observable and notifies listeners when its value changes.
    /// </summary>
    [ObservableProperty] private double _progress;

    /// <summary>
    /// Gets or sets the progress text displayed during the update process.
    /// This property is observable and initialized with a default value of "...".
    /// </summary>
    [ObservableProperty] private string _progressText = "...";
    
    /// <summary>
    /// Interaction used to handle the closing of the update window.
    /// </summary>
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();

    /// <summary>
    /// Command to close the update window.
    /// Invokes the <see cref="CloseWindowInteraction"/> to handle the close operation.
    /// </summary>
    [RelayCommand]
    public async Task CloseWindow() => await CloseWindowInteraction.Handle(Unit.Default);
}