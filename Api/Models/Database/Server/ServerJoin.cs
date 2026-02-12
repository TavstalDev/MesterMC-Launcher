using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Tavstal.MesterMC.Api.Models.Database.User;

namespace Tavstal.MesterMC.Api.Models.Database.Server;

public class ServerJoin
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    public ulong UserId { get; set; }
    
    public required string ServerInstanceId { get; set; }
    
    public required DateTimeOffset CreatedAt { get; set; }
    
    public required DateTimeOffset ExpiredAt { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public CustomUser? User { get; set; }
}