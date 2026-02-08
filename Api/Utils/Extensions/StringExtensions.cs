using System.Globalization;
using System.Net;

namespace Tavstal.MesterMC.Api.Utils.Extensions;

public static class StringExtensions
{
    public static bool EqualsIgnoreCase(this string str1, string str2)
    {
        return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
    }
    
    public static bool ContainsIgnoreCase(this string str, string part)
    {
        return CultureInfo.InvariantCulture.CompareInfo.IndexOf(str, part, CompareOptions.IgnoreCase) >= 0;
    }
    
    public static bool IsNullOrEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str);
    }
    
    public static string Capitalize(this string str)
    {
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToLowerInvariant());
    }
    
    public static bool IsLink(this string str)
    {
        if (str.IsNullOrEmpty())
            return false;

        return Uri.TryCreate(str, UriKind.Absolute, out Uri? uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    
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
    
    private static string DateTimeToIso2(int year, int month, int day, int hour, int minute, int second)
    {
        return new DateTime(year, month, day, hour, minute, second, 0, DateTimeKind.Local)
            .ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
    }
    
    public static string DateTimeToIso(DateTime dateTime)
    {
        return DateTimeToIso2(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
    }
}