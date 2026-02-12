using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Tavstal.MesterMC.Api.Models.Database.User;

public class UserCape
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    [StringLength(36)]
    public string UserId { get; set; }
    
    public ulong CapeId { get; set; }
    
    public bool IsSelected { get; set; }    
    
    public string? Reason { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset UpdatedAt { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("UserId")] 
    [JsonIgnore]
    public CustomUser User { get; set; } 
    
    [ForeignKey("CapeId")] 
    [JsonIgnore]
    public Cape Cape { get; set; }
}