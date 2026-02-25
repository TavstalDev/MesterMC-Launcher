using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Tavstal.MesterMC.Api.Models.Common;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8765 // Nullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).

namespace Tavstal.MesterMC.Api.Models.Database.User;

public sealed class CustomUser : IdentityUser<string>
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [StringLength(36)]
    public override string Id { get; set; }

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
    public override string UserName { get; set; }
    
    [StringLength(16)]
    public override string NormalizedUserName { get; set; }
    
    [StringLength(16)]
    public string? DisplayName { get; set; }
    
    public ESkinType SkinModel { get; set; }
    
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
    
    public CustomUser(string id, string userName, string normalizedUserName, string email, string normalizedEmail, string password, ESkinType skinModel, ulong? discordId, string? lockoutReason, DateTimeOffset createDate, DateTimeOffset lastUpdate, DateTimeOffset lastLogin)
    {
        Id = id;
        UserName = userName;
        NormalizedUserName = normalizedUserName;
        SkinModel = skinModel;
        DisplayName = userName; // Default display name is the username
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = password;
        DiscordId = discordId;
        LockoutReason = lockoutReason;
        CreateDate = createDate;
        LastUpdate = lastUpdate;
        LastLogin = lastLogin;
    }
    
    public CustomUser(string userName, string normalizedUserName, string email, string normalizedEmail, string password, ESkinType skinModel, ulong? discordId, string? lockoutReason, DateTimeOffset createDate, DateTimeOffset lastUpdate, DateTimeOffset lastLogin) : base(userName)
    {
        UserName = userName;
        NormalizedUserName = normalizedUserName;
        DisplayName = userName; // Default display name is the username
        Email = email;
        NormalizedEmail = normalizedEmail;
        PasswordHash = password;
        SkinModel = skinModel;
        DiscordId = discordId;
        LockoutReason = lockoutReason;
        CreateDate = createDate;
        LastUpdate = lastUpdate;
        LastLogin = lastLogin;
    }

    public string GetSkinVariant()
    {
        switch (SkinModel)
        {
            case ESkinType.WIDE:
                return "classic";
            case ESkinType.SLIM:
                return "slim";
        }
        return "classic";
    }
    
    public string HighestRole => UserRoles.OrderByDescending(x => x.Role?.Level).First().Role?.Name ?? "Anonymous";
    
    [JsonIgnore]
    public ICollection<CustomUserClaim> Claims { get; set; }
    
    [JsonIgnore]
    public ICollection<CustomUserToken> Tokens { get; set; }
    
    [JsonIgnore]
    public ICollection<CustomUserLogin> UserLogins { get; set; }
    
    public ICollection<CustomUserRole> UserRoles { get; set; }
    
    [JsonIgnore]
    public UserBillingInformation? BillingInformation { get; set; }
    
    [JsonIgnore]
    public ICollection<UserPlaySession> PlaySessions { get; set; }
    
    [JsonIgnore]
    public ICollection<FileData> Files { get; set; }
    
    [JsonIgnore]
    public ICollection<UserCape> OwnedCapes { get; set; }

    [JsonIgnore] 
    public FileData? Avatar => Files.FirstOrDefault(x => x.Type == EFileDataType.PROFILE_PICTURE);
    
    public bool HasAvatar => Files.Any(x => x.Type == EFileDataType.PROFILE_PICTURE);
    
    [JsonIgnore]
    public FileData? Skin => Files.FirstOrDefault(x => x.Type == EFileDataType.SKIN);

    [JsonIgnore] 
    public FileData? Cape => OwnedCapes.FirstOrDefault(x => x.IsSelected)?.Cape?.FileData;
}