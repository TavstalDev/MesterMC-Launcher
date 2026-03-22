namespace Tavstal.MesterMC.Updater.Models;

/// <summary>
/// Defines the logical windows (steps) used by the uninstaller UI.
/// </summary>
public enum EUninstallerWindow
{
    /// <summary>
    /// Intro/welcome screen shown at the start of the uninstall process.
    /// </summary>
    WELCOME = 0,
    
    /// <summary>
    /// Review screen where the user confirms choices before uninstalling.
    /// </summary>
    REVIEW = 1,
    
    /// <summary>
    /// Progress screen shown while uninstall operations are performed.
    /// </summary>
    PROGRESS = 2,
    
    /// <summary>
    /// Final screen displayed when the uninstall completes (or is cancelled).
    /// </summary>
    FINISH = 3
}