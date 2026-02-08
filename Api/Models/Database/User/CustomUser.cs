using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).

namespace Tavstal.MesterMC.Api.Models.Database.User;

public sealed class CustomUser : IdentityUser<ulong>
{
    [StringLength(320)]
    [ProtectedPersonalData]
    [EmailAddress]
    [JsonIgnore]
    public override string Email { get; set; }
    
    [StringLength(320)]
    [ProtectedPersonalData]
    [JsonIgnore]
    public override string NormalizedEmail { get; set; }
    
    [StringLength(256)]
    [ProtectedPersonalData]
    [JsonIgnore]
    public override string PasswordHash { get; set; }
    
    [StringLength(16)]
    [ProtectedPersonalData]
    public override string UserName { get; set; }
    
    [StringLength(16)]
    public override string NormalizedUserName { get; set; }
    
    [StringLength(16)]
    public string? DisplayName { get; set; }
    
    [StringLength(200)]
    [JsonIgnore]
    public string? AvatarPath { get; set; }
    
    public ulong? DiscordId { get; set; }
    
    [StringLength(200)]
    public string? LockoutReason { get; set; }
    
    [JsonIgnore]
    public override bool TwoFactorEnabled { get; set; }
    
    [StringLength(256)]
    [JsonIgnore]
    public string? TwoFactorSecret { get; set; }
    
    public DateTimeOffset CreateDate { get; set; }
    
    public DateTimeOffset LastUpdate { get; set; }
    
    public DateTimeOffset LastLogin { get; set; }

    #region JSON Ignored Identity Properties
    
    /// <summary>
    /// Gets or sets the security stamp of the user.
    /// This property is ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public override string? SecurityStamp { get; set; }

    /// <summary>
    /// Gets or sets the concurrency stamp of the user.
    /// This property is ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public override string? ConcurrencyStamp { get; set; }

    /// <summary>
    /// Gets or sets the phone number of the user.
    /// This property is ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public override string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the phone number is confirmed.
    /// This property is ignored during JSON serialization.
    /// </summary>
    [JsonIgnore]
    public override bool PhoneNumberConfirmed { get; set; }
    
    #endregion
    
    public CustomUser() { }
    
    public CustomUser(ulong id, string userName, string normalizedUserName, string email, string normalizedEmail, string password, string? avatarPath, ulong? discordId, string? lockoutReason, DateTimeOffset createDate, DateTimeOffset lastUpdate, DateTimeOffset lastLogin)
    {
        Id = id;
        UserName = userName;
        NormalizedUserName = normalizedUserName;
        DisplayName = userName; // Default display name is the username
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = password;
        AvatarPath = avatarPath;
        DiscordId = discordId;
        LockoutReason = lockoutReason;
        CreateDate = createDate;
        LastUpdate = lastUpdate;
        LastLogin = lastLogin;
    }
    
    public CustomUser(string userName, string normalizedUserName, string email, string normalizedEmail, string password, string? avatarPath, ulong? discordId, string? lockoutReason, DateTimeOffset createDate, DateTimeOffset lastUpdate, DateTimeOffset lastLogin) : base(userName)
    {
        UserName = userName;
        NormalizedUserName = normalizedUserName;
        DisplayName = userName; // Default display name is the username
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = password;
        AvatarPath = avatarPath;
        DiscordId = discordId;
        LockoutReason = lockoutReason;
        CreateDate = createDate;
        LastUpdate = lastUpdate;
        LastLogin = lastLogin;
    }
    
    public bool HasAvatar => !string.IsNullOrEmpty(AvatarPath) && File.Exists(AvatarPath);
    
    public string HighestRole => UserRoles.OrderByDescending(x => x.Role?.Level).First().Role?.Name ?? "Anonymous";
    
    [JsonIgnore]
    public ICollection<CustomUserClaim> Claims { get; set; }
    
    [JsonIgnore]
    public ICollection<CustomUserToken> Tokens { get; set; }
    
    [JsonIgnore]
    public ICollection<CustomUserLogin> UserLogins { get; set; }
    
    public ICollection<CustomUserRole> UserRoles { get; set; }
    
    [JsonIgnore]
    public ICollection<UserSession> UserSessions { get; set; }
    
    public UserSkin Skin { get; set; }
    
    [JsonIgnore]
    public UserBillingInformation? BillingInformation { get; set; }
}