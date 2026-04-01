using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Updater.Views.Models;

namespace Tavstal.MesterMC.Updater.Views;

/// <summary>
/// Window used for uninstall operations.
/// Binds to <see cref="UninstallViewModel"/> and implements <see cref="IProgressReporter"/>
/// to allow the view model or external code to report progress and status text to the UI.
/// </summary>
public partial class UninstallWindow : KonkordWindow<UninstallViewModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UninstallWindow"/> class.
    /// Sets up the DataContext with an <see cref="UninstallViewModel"/> if none is provided,
    /// attaches debug tools in DEBUG builds, and registers activation handlers for window closing.
    /// </summary>
    public UninstallWindow()
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        DataContext ??= new UninstallViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.CloseWindowInteraction.RegisterHandler(action =>
            {
                Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
}