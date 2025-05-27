using Avalonia.Data.Converters;
using SPTInstaller.CustomControls;
using System.Globalization;

namespace SPTInstaller.Converters;

public class StatusSpinnerIsProcessingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not StatusSpinner.SpinnerState state)
            return null;
        
        
        if (parameter is string parm && parm == "invert")
        {
            return state > 0;
        }
        
        return state <= 0;
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}