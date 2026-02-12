using System.ComponentModel.DataAnnotations;

namespace Tavstal.MesterMC.Api.Models.Database.Server;

public class Server
{
    [Key]
    public long Id { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset? DeletedAt { get; set; }
}