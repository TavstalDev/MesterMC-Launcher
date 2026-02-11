namespace Tavstal.MesterMC.Api.Utils;

public static class YiggdrasilHelper
{
    private static string Sign(string value)
    {
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value));
    }

    public static List<Dictionary<string, object>> Properties(params KeyValuePair<string, string>[] entries)
    {
        return Properties(false, entries);
    }

    public static List<Dictionary<string, object>> Properties(bool sign, params KeyValuePair<string, string>[] entries)
    {
        return entries.Select(entry =>
        {
            var property = new Dictionary<string, object>
            {
                ["name"] = entry.Key,
                ["value"] = entry.Value
            };

            if (sign)
            {
                property["signature"] = Sign(entry.Value);
            }

            return property;
        }).ToList();
    }
}