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
    public required string UserId { get; set; }
    
    public required ulong CapeId { get; set; }
    
    public bool IsSelected { get; set; }    
    
    public string? Reason { get; set; }
    
    public required DateTimeOffset CreatedAt { get; set; }
    
    public required DateTimeOffset UpdatedAt { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("UserId")] 
    [JsonIgnore]
    public CustomUser? User { get; set; } 
    
    [ForeignKey("CapeId")] 
    [JsonIgnore]
    public Cape? Cape { get; set; }
}