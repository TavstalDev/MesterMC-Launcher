using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Tavstal.MesterMC.Api.Models.Database.User;

public class UserBackupCode
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    [StringLength(36)]
    public string UserId { get; set; }
    
    [StringLength(256)]
    public string HashedCode { get; set; }
    
    public DateTimeOffset CreateAt { get; set; }
    
    public DateTimeOffset? UsedAt { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    /// <summary>
    /// Gets or sets the user associated with this UserCape record.
    /// This property is ignored during JSON serialization.
    /// </summary>
    [ForeignKey("UserId")] 
    [JsonIgnore]
    public CustomUser? User { get; set; } 
}