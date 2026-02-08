using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace Tavstal.MesterMC.Api.Models.Database.User;

public sealed class CustomUserLogin : IdentityUserLogin<ulong>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public ulong Id { get; set; }
    
    public override ulong UserId { get; set; }
    
    [StringLength(255)]
    public override string? ProviderKey { get; set; }
    
    [StringLength(32)]
    public override string ProviderDisplayName { get; set; }
    
    [StringLength(64)]
    public override string LoginProvider { get; set; }
    
    [StringLength(15)]
    // ReSharper disable once InconsistentNaming
    public string? IPv4Address { get; set; }
    
    [StringLength(40)]
    // ReSharper disable once InconsistentNaming
    public string? IPv6Address { get; set; }
    
    [StringLength(256)]
    public string? Country { get; set; }
    
    [StringLength(256)]
    public string? City { get; set; }
    
    [StringLength(256)]
    public string? Region { get; set; }
    
    [StringLength(256)]
    public string? OperatingSystem { get; set; }
    
    [StringLength(256)]
    public string? Browser { get; set; }
    
    public DateTimeOffset CreateDate { get; set; }
    
    public DateTimeOffset ExpireDate { get; set; }
    
    public CustomUserLogin() {}
    
    public CustomUserLogin(ulong userId, string? providerKey, string providerDisplayName, string loginProvider, string ipv4Address, string ipv6Address, string country, string city, string region, string operatingSystem, string browser, DateTimeOffset createDate, DateTimeOffset expireDate)
    {
        UserId = userId;
        ProviderKey = providerKey;
        ProviderDisplayName = providerDisplayName;
        LoginProvider = loginProvider;
        IPv4Address = ipv4Address;
        IPv6Address = ipv6Address;
        Country = country;
        City = city;
        Region = region;
        OperatingSystem = operatingSystem;
        Browser = browser;
        CreateDate = createDate;
        ExpireDate = expireDate;
    }
    
    public CustomUserLogin(ulong userId, string? providerKey, string providerDisplayName, string loginProvider, string? ipv4Address, string? ipv6Address, IpInfo ipInfo, string operatingSystem, string browser, DateTimeOffset createDate, DateTimeOffset expireDate)
    {
        UserId = userId;
        ProviderKey = providerKey;
        ProviderDisplayName = providerDisplayName;
        LoginProvider = loginProvider;
        IPv4Address = ipv4Address;
        IPv6Address = ipv6Address;
        Country = ipInfo.Country;
        City = ipInfo.City;
        Region = ipInfo.Region;
        OperatingSystem = operatingSystem;
        Browser = browser;
        CreateDate = createDate;
        ExpireDate = expireDate;
    }

    /* ######################################################################
     *                         NAVIGATION PROPERTIES
     * ###################################################################### */

    /// <summary>
    /// Gets or sets the user associated with the notification.
    /// </summary>
    [ForeignKey("UserId")]
    [JsonIgnore]
    public CustomUser? User { get; set; }
}