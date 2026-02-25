using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tavstal.MesterMC.Api.Models.Database.Launcher;

public class LauncherVersionData
{
    [Key]
    public ulong Id { get; set; }
    
    public ulong VersionId { get; set; }
    
    public ulong FileId { get; set; }
    
    public ELauncherOs Os { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("VersionId")]
    public LauncherVersion Version { get; set; }
    
    [ForeignKey("FileId")]
    public FileData File { get; set; }
}