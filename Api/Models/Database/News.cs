using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

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
    
    public ulong BannerId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("BannerId")]
    [JsonIgnore]
    public FileData? Banner { get; set; }
}