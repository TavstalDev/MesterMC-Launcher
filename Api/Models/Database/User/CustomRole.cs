using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Tavstal.MesterMC.Api.Models.Database.User;

public class CustomRole : IdentityRole<string>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [StringLength(36)]
    public override string Id { get; set; }
    
    public byte Level { get; set; }
    
    [StringLength(32)]
    public new string Name { get; set; }
    
    [StringLength(64)]
    public new string NormalizedName { get; set; }
    
    [StringLength(36)]
    public new string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
    
    public CustomRole(byte level, string name, string normalizedName)
    {
        Level = level;
        Name = name;
        NormalizedName = normalizedName;
    }
    
    public CustomRole(string roleName, byte level, string name, string normalizedName) : base(roleName)
    {
        Level = level;
        Name = name;
        NormalizedName = normalizedName;
    }
}