using System.ComponentModel.DataAnnotations;

namespace Tavstal.MesterMC.Api.Models.Bodies.Account;

public class UpdateBillingInfoRequestBody
{
    [Required]
    public required string FirstName { get; set; }
    
    [Required]
    public required string LastName { get; set; }
    
    [Required]
    public required string AddressOne { get; set; }
    
    public string? AddressTwo { get; set; }
    
    public string? City { get; set; }
    
    [Required]
    public required string PostalCode { get; set; }
    
    [Required]
    public required string Country { get; set; }
}