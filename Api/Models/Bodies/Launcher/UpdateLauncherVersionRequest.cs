using System.ComponentModel.DataAnnotations;
using Tavstal.MesterMC.Api.Models.Database.Launcher;

namespace Tavstal.MesterMC.Api.Models.Bodies.Launcher;

public class UpdateLauncherVersionRequest
{
    [StringLength(15)]
    [RegularExpression("^(?:(\\d+)\\.)?(?:(\\d+)\\.)?(\\*|\\d+)$\n")]
    public string? Version { get; set; }
    
    public EVersionType? VersionType { get; set; }
    
    [StringLength(500)]
    public string? Changelog { get; set; }
}