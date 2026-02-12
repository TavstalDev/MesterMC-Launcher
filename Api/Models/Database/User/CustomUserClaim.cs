using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace Tavstal.MesterMC.Api.Models.Database.User;

public class CustomUserClaim : IdentityUserClaim<string>
{
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public CustomUser? User { get; set; }
}