using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Tavstal.MesterMC.Api.Models.Database.User;

public sealed class CustomUserToken : IdentityUserToken<ulong>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; }
    
    public override ulong UserId { get; set; }
    
    [StringLength(32)]
    public override string Name { get; set; }
    
    [StringLength(255)]
    [Required]
    public override string Value { get; set; }
    
    [StringLength(32)]
    public override string LoginProvider { get; set; }
    
    public DateTimeOffset CreateDate { get; set; }
    
    public CustomUserToken() { }
    
    public CustomUserToken(ulong userId, string name, string value, string loginProvider, DateTimeOffset createDate)
    {
        UserId = userId;
        Name = name;
        Value = value;
        LoginProvider = loginProvider;
        CreateDate = createDate;
    }

    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public CustomUser? User { get; set; }
}