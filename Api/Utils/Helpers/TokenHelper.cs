using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Tavstal.MesterMC.Api.Utils.Extensions;

namespace Tavstal.MesterMC.Api.Utils.Helpers;

/// <summary>
/// Provides utility methods for generating various types of tokens.
/// </summary>
public static class TokenHelper
{
    /// <summary>
    /// Generates a random string using the specified character set and length.
    /// </summary>
    /// <param name="charSet">The set of characters to use for the random string.</param>
    /// <param name="length">The length of the random string.</param>
    /// <returns>A randomly generated string.</returns>
    public static string GenerateRandomString(string charSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", int length = 64)
    {
        string key = "";
        for (int i = 0; i < length; i++)
            key += charSet.ElementAt(MathExtensions.Next(0, charSet.Length - 1));

        return key;
    }

    /// <summary>
    /// Generates a JSON Web Token (JWT) using the specified parameters.
    /// </summary>
    /// <param name="encryptionKey">The encryption key used to sign the token.</param>
    /// <param name="issuer">The issuer of the token.</param>
    /// <param name="audience">The audience for which the token is intended.</param>
    /// <param name="expireDate">The expiration date of the token.</param>
    /// <param name="claims">Optional claims to include in the token.</param>
    /// <returns>A string representation of the generated JWT.</returns>
    public static string GenerateJwtToken(string encryptionKey, string issuer, string audience, DateTimeOffset expireDate, IEnumerable<Claim>? claims = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(encryptionKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            new JwtHeader(credentials),
            new JwtPayload(issuer, audience, claims, DateTime.UtcNow, expireDate.Date)
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generates a random token with the specified byte length.
    /// </summary>
    /// <param name="tokenByteLength">The length of the token in bytes.</param>
    /// <returns>A base64-encoded string representation of the token.</returns>
    public static string GenerateToken(int tokenByteLength = 48)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(tokenByteLength);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
    
    /// <summary>
    /// Generates an account confirmation token.
    /// </summary>
    /// <returns>An account confirmation token as a base64-encoded string.</returns>
    public static string GenerateAccountConfirmationToken() => GenerateToken();

    /// <summary>
    /// Generates a recovery token.
    /// </summary>
    /// <returns>A recovery token as a base64-encoded string.</returns>
    public static string GenerateRecoveryToken() => GenerateToken();

    /// <summary>
    /// Generates a two-factor authentication token.
    /// </summary>
    /// <returns>A two-factor authentication token as a base64-encoded string.</returns>
    public static string GenerateTwoFactorToken() => GenerateToken(20);

    /// <summary>
    /// Generates a two-factor session token.
    /// </summary>
    /// <returns>A two-factor session token as a base64-encoded string.</returns>
    public static string GenerateTwoFactorSessionToken() => GenerateToken(32);
    
    /// <summary>
    /// Generates a recovery code.
    /// </summary>
    /// <returns>A recovery code as a base64-encoded string.</returns>
    public static string GenerateRecoveryCode() => GenerateToken(8);
    
    /// <summary>
    /// Generates a list of recovery codes.
    /// </summary>
    /// <param name="count">The number of recovery codes to generate.</param>
    /// <returns>A list of recovery codes.</returns>
    public static List<string> GenerateRecoveryCodes(int count = 8)
    {
        List<string> recoveryCodes = [];
        for (int i = 0; i < count; i++)
            recoveryCodes.Add(GenerateToken(8));
        return recoveryCodes;
    }

    /// <summary>
    /// Generates a random password.
    /// </summary>
    /// <returns>A randomly generated password as a base64-encoded string.</returns>
    public static string GeneratePassword() => GenerateToken(16);
}