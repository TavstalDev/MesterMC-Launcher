using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Tavstal.MesterMC.Api.Models.Database.User;

public class UserPlaySession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    [StringLength(36)]
    public required string UserId { get; set; }
    
    [StringLength(64)]
    public required string Token { get; set; }
    
    [StringLength(50)]
    public required string UserIp { get; set; }
    
    public required DateTimeOffset CreatedAt { get; set; }
    
    public required DateTimeOffset ExpiresAt { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public CustomUser? User { get; set; }
}