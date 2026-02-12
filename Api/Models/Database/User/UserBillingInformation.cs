using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Tavstal.MesterMC.Api.Models.Database.User;

public class UserBillingInformation
{
    [Key]
    [StringLength(36)]
    public string UserId { get; set; }
    
    [PersonalData]
    [StringLength(50)]
    public string FirstName { get; set; }
    
    [PersonalData]
    [StringLength(50)]
    public string LastName { get; set; }
    
    [PersonalData]
    [StringLength(255)]
    public string AddressOne { get; set; }
    
    [PersonalData]
    [StringLength(255)]
    public string? AddressTwo { get; set; }
    
    [PersonalData]
    [StringLength(100)]
    public string? City { get; set; }
    
    [PersonalData]
    [StringLength(20)]
    public string PostalCode { get; set; }
    
    [PersonalData]
    [StringLength(100)]
    public string Country { get; set; }
    
    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */

    /// <summary>
    /// Gets or sets the user associated with the billing information.
    /// </summary>
    [ForeignKey("UserId")]
    [JsonIgnore]
    public CustomUser? User { get; set; }
}