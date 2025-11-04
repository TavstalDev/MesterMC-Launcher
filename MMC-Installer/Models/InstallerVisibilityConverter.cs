using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Tavstal.MesterMC.Installer.Models;

public class InstallerVisibilityConverter<TEnum> : IValueConverter where TEnum : Enum
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EInstallerWindow enumValue && parameter is string parameterString)
        {
            if (Enum.TryParse(parameterString, out EInstallerWindow targetEnum))
            {
                // Returns true if the bound enum value matches the parameter enum value
                return enumValue.Equals(targetEnum);
            }
        }
        // Default to false or throw an error based on your preference
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // One-way conversion is usually sufficient for Visibility binding
        return new BindingNotification(new NotSupportedException(), BindingErrorType.Error);
    }
}