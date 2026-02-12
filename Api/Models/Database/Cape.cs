using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tavstal.MesterMC.Api.Models.Database;

public class Cape
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    public ulong FileId { get; set; }
    
    public bool IsPublic { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */
    
    [ForeignKey("FileId")] 
    public FileData FileData { get; set; }
}