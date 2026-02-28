using Tavstal.MesterMC.Api.Models.Attributes;
using Tavstal.MesterMC.Api.Models.Database.Launcher;

namespace Tavstal.MesterMC.Api.Models.Bodies.Launcher;

public class CreateLauncherVersionDataRequest
{
    public required ELauncherOs Os { get; set; }
    
    [FormFile(maxFileSize: 512, fileExtensions: [".zip", ".tar.gz", ".tar"], contentTypes: ["application/zip", "application/gzip", "application/x-tar"])]
    public required IFormFile File { get; set; }
}