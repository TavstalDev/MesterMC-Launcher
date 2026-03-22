using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using Avalonia;
using ReactiveUI;
using Tavstal.KonkordLauncher.Common.Models;
using Tavstal.KonkordLauncher.Core.Models;
using Tavstal.MesterMC.Updater.Views.Models;

namespace Tavstal.MesterMC.Updater.Views;

public partial class UninstallWindow : KonkordWindow<UninstallViewModel>, IProgressReporter
{
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
    
    #region IProgressReporter Implementation
    /// <summary>
    /// Sets the progress value for the startup process.
    /// </summary>
    /// <param name="progress">The progress value, typically between 0.0 and 1.0.</param>
    public void SetProgress(double progress)
    {
        if (DataContext == null)
            return;
        DataContext.InstallProgress = progress;
    }

    /// <summary>
    /// Sets the status message for the startup process.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    public void SetStatus(string status)
    {
        if (DataContext == null)
            return;
        DataContext.InstallText = status;
    }

    /// <summary>
    /// Sets the status message with optional arguments.
    /// </summary>
    /// <param name="status">The status message to display.</param>
    /// <param name="args">Optional arguments for formatting the status message.</param>
    public void SetStatus(string status, params object[]? args)
    {
        if (DataContext == null)
            return;
        if (args == null || args.Length == 0)
        {
            DataContext.InstallText = status;
            return;
        }
        DataContext.InstallText = string.Format(status, args);
    }
    #endregion 
}