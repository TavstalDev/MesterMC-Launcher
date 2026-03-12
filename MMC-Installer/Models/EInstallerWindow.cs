namespace Tavstal.MesterMC.Installer.Models;

/// <summary>
/// Represents the individual steps/windows shown by the installer UI.
/// </summary>
public enum EInstallerWindow
{
    /// <summary>
    /// Initial welcome/introduction page shown to the user.
    /// </summary>
    WELCOME = 0,

    /// <summary>
    /// Licence / EULA acceptance page where the user must agree to terms.
    /// </summary>
    LICENSE = 1,

    /// <summary>
    /// Page that allows the user to select installation location / folder.
    /// </summary>
    LOCATION_SELECT = 2,

    /// <summary>
    /// Optional shortcuts creation page (desktop/start menu/none).
    /// </summary>
    SHORTCUTS = 3,

    /// <summary>
    /// Final review page summarizing chosen options before installation begins.
    /// </summary>
    REVIEW = 4,

    /// <summary>
    /// Installation progress page shown while files are being copied / installed.
    /// </summary>
    INSTALLING = 5,

    /// <summary>
    /// Completion page shown when installation finishes (success or failure summary).
    /// </summary>
    FINISHED = 6
}