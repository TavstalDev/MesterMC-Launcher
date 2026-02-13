using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models.Database.User;

namespace Tavstal.MesterMC.Api.Models.Database;

public class FileData
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    [StringLength(64)]
    public string Hash { get; set; }
    
    [StringLength(255)]
    public string FileName { get; set; }
    
    [StringLength(100)]
    public string ContentType { get; set; }
    
    public EFileDataType Type { get; set; }
    
    // Id of the file's owner, if any
    [StringLength(36)]
    public string? UserId { get; set; }

    private string GetContainingDirectoryName()
    {
        switch (Type)
        {
            case EFileDataType.PROFILE_PICTURE:
                return "avatars";
            case EFileDataType.SKIN:
                return "skins";
            case EFileDataType.CAPE:
                return "capes";
            default:
                return "misc";
        }
    }
    
    public bool Exists()
    {
        string dirPath = Startup.UploadDirectory;
        if (!Directory.Exists(dirPath))
            return false;
        
        string filePath = Path.Combine(dirPath, GetContainingDirectoryName(), FileName);
        return File.Exists(filePath);
    }
    
    public byte[]? GetFileData()
    {
        if (!Exists())
            return null;
        
        string dirPath = Startup.UploadDirectory;
        string filePath = Path.Combine(dirPath, GetContainingDirectoryName(), FileName);
        return File.ReadAllBytes(filePath);
    }

    public void SaveFile(Stream stream)
    {
        string dirPath = Startup.UploadDirectory;
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        
        string containingDir = GetContainingDirectoryName();
        string containingDirPath = Path.Combine(dirPath, containingDir);
        if (!Directory.Exists(containingDirPath))
            Directory.CreateDirectory(containingDirPath);
        
        string filePath = Path.Combine(containingDirPath, FileName);
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        stream.CopyTo(fileStream);
    }
    
    public void DeleteFile()
    {
        if (!Exists())
            return;
        
        string dirPath = Startup.UploadDirectory;
        string filePath = Path.Combine(dirPath, GetContainingDirectoryName(), FileName);
        File.Delete(filePath);
    }

    public string GetUrl(string baseUrl)
    {
        return $"{baseUrl}/yggdrasil/textures/{Hash}";
    }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public CustomUser? User { get; set; }
}