using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tavstal.MesterMC.Api.Models.Database;

public class FileData
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    
    [StringLength(64)]
    public string Hash { get; set; }
    
    [StringLength(255)]
    public string FileName { get; set; }
    
    [StringLength(100)]
    public string ContentType { get; set; }

    public bool Exists()
    {
        string dirPath = Startup.UploadDirectory;
        if (!Directory.Exists(dirPath))
            return false;
        
        string filePath = Path.Combine(dirPath, FileName);
        return File.Exists(filePath);
    }
    
    public byte[]? GetFileData()
    {
        if (!Exists())
            return null;
        
        string dirPath = Startup.UploadDirectory;
        string filePath = Path.Combine(dirPath, FileName);
        return File.ReadAllBytes(filePath);
    }
}