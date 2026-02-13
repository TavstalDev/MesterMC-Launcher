using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Tavstal.MesterMC.Api.Models.Database.User;

public class CustomUserRole : IdentityUserRole<string>
{
    [StringLength(36)]
    public override string UserId { get; set; }
    
    [StringLength(36)]
    public override string RoleId { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public CustomUser? User { get; set; }
    
    [ForeignKey("RoleId")]
    [JsonIgnore]
    public CustomRole? Role { get; set; }
}