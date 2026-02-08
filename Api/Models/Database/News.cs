using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tavstal.MesterMC.Api.Models.Database;

public class News
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    [StringLength(128)]
    public string Title { get; set; }
    
    [StringLength(512)]
    public string Content { get; set; }
    
    [StringLength(64)]
    public string Banner { get; set; }
    
    public DateTime CreatedAt { get; set; }
}