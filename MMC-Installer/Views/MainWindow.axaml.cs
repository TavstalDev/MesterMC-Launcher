using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Installer.Views;

namespace Tavstal.MesterMC.Installer.Views;

public partial class MainWindow : KonkordWindow<MainViewModel>
{
    private readonly CoreLogger _logger = CoreLogger.WithModuleType(typeof(MainWindow));
    
    public MainWindow()
    {
        InitializeComponent();
        
#if DEBUG
        // Attaches Avalonia Dev Tools for debugging purposes.
        this.AttachDevTools();
#endif
        
        DataContext ??= new MainViewModel();
        this.WhenActivated(disposables =>
        {
            DataContext.CloseInteraction.RegisterHandler(action =>
            {
                this.Close();
                action.SetOutput(Unit.Default);
                return Task.CompletedTask;
            }).DisposeWith(disposables);
        });
    }
}