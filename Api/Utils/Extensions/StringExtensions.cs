using System.Globalization;
using System.Net;

namespace Tavstal.MesterMC.Api.Utils.Extensions;

/// <summary>
/// Provides extension methods for string operations, including case-insensitive comparisons,
/// validation checks, and formatting utilities.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Compares two strings for equality, ignoring case.
    /// </summary>
    /// <param name="str1">The first string to compare.</param>
    /// <param name="str2">The second string to compare.</param>
    /// <returns>True if the strings are equal ignoring case; otherwise, false.</returns>
    public static bool EqualsIgnoreCase(this string str1, string str2)
    {
        return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
    }
    
    /// <summary>
    /// Checks if a string contains a specified substring, ignoring case.
    /// </summary>
    /// <param name="str">The string to search within.</param>
    /// <param name="part">The substring to search for.</param>
    /// <returns>True if the substring is found; otherwise, false.</returns>
    public static bool ContainsIgnoreCase(this string str, string part)
    {
        return CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, part, CompareOptions.IgnoreCase) >= 0;
    }
    
    /// <summary>
    /// Determines whether a string is null or empty.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string is null or empty; otherwise, false.</returns>
    public static bool IsNullOrEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str);
    }
    
    /// <summary>
    /// Capitalizes the first letter of each word in the string.
    /// </summary>
    /// <param name="str">The string to capitalize.</param>
    /// <returns>The capitalized string.</returns>
    public static string Capitalize(this string str)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToLowerInvariant());
    }
    
    /// <summary>
    /// Determines whether a string is a valid HTTP or HTTPS link.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>True if the string is a valid link; otherwise, false.</returns>
    public static bool IsLink(this string str)
    {
        if (str.IsNullOrEmpty())
            return false;

        return Uri.TryCreate(str, UriKind.Absolute, out Uri? uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    
    /// <summary>
    /// Checks if a string is a valid and accessible HTTP or HTTPS link.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>True if the string is a valid and accessible link; otherwise, false.</returns>
    public static bool IsValidLink(this string str)
    {
        if (!str.IsLink())
            return false;

        using HttpClient client = new HttpClient();
        HttpResponseMessage result = client.GetAsync(str).Result;
        HttpStatusCode statusCode = result.StatusCode;

        switch (statusCode)
        {
            case HttpStatusCode.Accepted:
            case HttpStatusCode.OK:
                return true;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// Determines whether a string is a valid email address.
    /// </summary>
    /// <param name="str">The string to validate.</param>
    /// <returns>True if the string is a valid email address; otherwise, false.</returns>
    public static bool IsValidEmail(this string str)
    {
        if (str.IsNullOrEmpty())
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(str);
            return addr.Address == str;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    /// <summary>
    /// Converts a number to a shortened string representation (e.g., "1K" for 1000).
    /// </summary>
    /// <param name="num">The number to shorten.</param>
    /// <returns>The shortened string representation of the number.</returns>
    public static string ShortifyNumber(this int num)
    {
        if (num >= 100000)
            return ShortifyNumber(num / 1000) + "K";

        if (num >= 10000)
            return (num / 1000D).ToString("0.#") + "K";

        if (num >= 1000)
            return (num / 1000D).ToString("0.#") + "K";

        return num.ToString("#,0");
    }
    
    /// <summary>
    /// Converts the specified date and time components to an ISO 8601 string.
    /// </summary>
    /// <param name="year">The year component.</param>
    /// <param name="month">The month component.</param>
    /// <param name="day">The day component.</param>
    /// <param name="hour">The hour component.</param>
    /// <param name="minute">The minute component.</param>
    /// <param name="second">The second component.</param>
    /// <returns>The ISO 8601 string representation of the date and time.</returns>
    private static string DateTimeToIso2(int year, int month, int day, int hour, int minute, int second)
    {
        return new DateTime(year, month, day, hour, minute, second, 0, DateTimeKind.Local)
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
    }
    
    /// <summary>
    /// Converts a DateTime object to an ISO 8601 string.
    /// </summary>
    /// <param name="dateTime">The DateTime object to convert.</param>
    /// <returns>The ISO 8601 string representation of the DateTime.</returns>
    public static string DateTimeToIso(DateTime dateTime)
    {
        return DateTimeToIso2(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
    }
}