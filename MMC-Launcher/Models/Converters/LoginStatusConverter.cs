using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Tavstal.MesterMC.Launcher.Helpers;

namespace Tavstal.MesterMC.Launcher.Models.Converters;

/// <summary>
/// Converts login status values to corresponding image resources for display purposes.
/// </summary>
public class LoginStatusConverter : IValueConverter
{
    /// <summary>
    /// Converts an <see cref="ELoginStatus"/> value to an image resource path.
    /// </summary>
    /// <param name="value">The value to convert, expected to be of type <see cref="ELoginStatus"/>.</param>
    /// <param name="targetType">The target type, expected to be assignable to <see cref="IImage"/>.</param>
    /// <param name="parameter">An optional parameter for the conversion (not used).</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>
    /// An image resource loaded from the path corresponding to the <see cref="ELoginStatus"/> value,
    /// or null if the value is not matched.
    /// </returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ELoginStatus zoneState && targetType.IsAssignableTo(typeof(IImage)))
        {
            // Determine the resource path based on the enum value
            string path = zoneState switch
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

    /// <summary>
    /// Converts back from the target type to the source type. Not implemented.
    /// </summary>
    /// <param name="value">The value to convert back.</param>
    /// <param name="targetType">The target type to convert to.</param>
    /// <param name="parameter">An optional parameter for the conversion.</param>
    /// <param name="culture">The culture information for the conversion.</param>
    /// <returns>Throws a <see cref="NotImplementedException"/>.</returns>
    /// <exception cref="NotImplementedException">Thrown because this method is not implemented.</exception>
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}