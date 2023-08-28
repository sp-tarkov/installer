using Avalonia.Data.Converters;
using SPTInstaller.CustomControls;
using System.Globalization;

namespace SPTInstaller.Converters;
public class StatusSpinnerIsStateConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) 
            return null;

        if (value is not StatusSpinner.SpinnerState state)
            return null;

        if (parameter is not string stateName)
            return null;

        return state.ToString().ToLower() == stateName.ToLower();
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}
