using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReactiveUI;

namespace Tavstal.MesterMC.Launcher.Views.Models;

public partial class LauncherViewModel : ObservableObject
{
    public Interaction<Unit, Unit> CloseWindowInteraction { get; } = new();
    
    [RelayCommand]
    public async Task CloseWindow()
    {
        await CloseWindowInteraction.Handle(Unit.Default);
    }
}