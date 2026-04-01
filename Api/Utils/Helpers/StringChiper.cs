using System.Security.Cryptography;
using System.Text;

namespace Tavstal.MesterMC.Api.Utils.Helpers;

/// <summary>
/// Utility class providing cryptographic operations for string hashing and authentication.
/// </summary>
public static class StringChiper
{
    /// <summary>
    /// Converts a string to its UTF-8 hexadecimal representation.
    /// </summary>
    /// <param name="input">The input string to convert to hexadecimal.</param>
    /// <returns>The hexadecimal representation of the UTF-8 encoded input string in lowercase.</returns>
    public static string GetHash(string input)
    {
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToHexStringLower(inputBytes);
    }
    
    /// <summary>
    /// Computes an HMAC-SHA256 keyed hash of the input string.
    /// </summary>
    /// <param name="input">The plaintext string to hash.</param>
    /// <param name="key">The secret key for HMAC computation. Should be stored securely and treated as a credential.</param>
    /// <returns>A hexadecimal string (lowercase) representation of the computed HMAC-SHA256 hash.</returns>
    public static string GetEncryptedHash(string input, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        byte[] hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(hashBytes);
    }
}