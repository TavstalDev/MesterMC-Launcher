using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Tavstal.MesterMC.Launcher.Helpers;

namespace Tavstal.MesterMC.Launcher.Models;

public class LoginStatusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ELoginStatus zoneState && targetType.IsAssignableTo(typeof(IImage)))
        {
            // Determine the resource path based on the enum value
            string? path = zoneState switch
            {
                ELoginStatus.LOGGING_IN => "avares://MMC-Launcher/Assets/zone/zone-logging.png",
                ELoginStatus.SUCCESS => "avares://MMC-Launcher/Assets/zone/zone-login.png",
                ELoginStatus.LAUNCHING => "avares://MMC-Launcher/Assets/zone/zone-data.png",
                _ => "avares://MMC-Launcher/Assets/zone/zone-logging.png"
            };

            var uri = new Uri(path);
            return ImageHelper.LoadFromResource(uri);
        }

        return null; // Return null if the enum value is not matched
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}