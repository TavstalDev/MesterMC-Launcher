using System.ComponentModel.DataAnnotations;
using Tavstal.MesterMC.Api.Models.Database.Launcher;

namespace Tavstal.MesterMC.Api.Models.Bodies.Launcher;

public class CreateLauncherVersionRequest
{
    [StringLength(15)]
    [RegularExpression("^(?:(\\d+)\\.)?(?:(\\d+)\\.)?(\\*|\\d+)$\n")]
    public required string Version { get; set; }
    
    public required EVersionType VersionType { get; set; }
    
    [StringLength(500)]
    public required string Changelog { get; set; }
}