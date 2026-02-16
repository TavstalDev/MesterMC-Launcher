using System.Security.Cryptography;
using System.Text;

namespace Tavstal.MesterMC.Api.Utils.Helpers;

/// <summary>
/// Provides utility methods for hashing, encryption, and decryption of strings.
/// </summary>
public static class StringChiper
{
    /// <summary>
    /// Computes the SHA-256 hash of the given input string.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <returns>The SHA-256 hash as a hexadecimal string.</returns>
    public static string GetSha256Hash(string input)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = sha256.ComputeHash(bytes);

        // Convert hash to hex string
        return Convert.ToHexString(hash);
    }
    
    /// <summary>
    /// Computes the SHA-256 hash of the encrypted input string using the provided key.
    /// </summary>
    /// <param name="input">The input string to hash.</param>
    /// <param name="key">The encryption key.</param>
    /// <returns>The SHA-256 hash of the encrypted input.</returns>
    public static string GetEncryptedSha256Hash(string input, string key)
    {
        return GetSha256Hash(Encrypt(key, input));
    }
    
    /// <summary>
    /// Encrypts the given plaintext string using the specified key.
    /// </summary>
    /// <param name="key">The encryption key.</param>
    /// <param name="plainText">The plaintext string to encrypt.</param>
    /// <returns>The encrypted string in Base64 format.</returns>
    public static string Encrypt(string key, string plainText)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    streamWriter.Write(plainText);

                array = memoryStream.ToArray();
            }
        }

        return Convert.ToBase64String(array);
    }
    
    /// <summary>
    /// Encrypts the given plaintext string using the specified key (extension method).
    /// </summary>
    /// <param name="plainText">The plaintext string to encrypt.</param>
    /// <param name="key">The encryption key.</param>
    /// <returns>The encrypted string in Base64 format.</returns>
    public static string EncryptSelf(this string plainText, string key)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    streamWriter.Write(plainText);
                array = memoryStream.ToArray();
            }
        }

        return Convert.ToBase64String(array);
    }
    
    /// <summary>
    /// Decrypts the given ciphertext string using the specified key (extension method).
    /// </summary>
    /// <param name="cipherText">The encrypted string in Base64 format.</param>
    /// <param name="key">The decryption key.</param>
    /// <returns>The decrypted plaintext string, or the original ciphertext if decryption fails.</returns>
    public static string DecryptSelf(this string cipherText, string key)
    {
        try
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new MemoryStream(buffer);
            using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new StreamReader(cryptoStream);
            return streamReader.ReadToEnd();
        }
        catch
        {
            return cipherText;
        }
    }
    
    /// <summary>
    /// Decrypts the given ciphertext string using the specified key.
    /// </summary>
    /// <param name="key">The decryption key.</param>
    /// <param name="cipherText">The encrypted string in Base64 format.</param>
    /// <returns>The decrypted plaintext string.</returns>
    public static string Decrypt(string key, string cipherText)
    {
        byte[] iv = new byte[16];
        byte[] buffer = Convert.FromBase64String(cipherText);

        using Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = iv;
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using MemoryStream memoryStream = new MemoryStream(buffer);
        using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using StreamReader streamReader = new StreamReader(cryptoStream);
        return streamReader.ReadToEnd();
    }
}