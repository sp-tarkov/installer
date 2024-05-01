using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using SPTInstaller.CustomControls;

namespace SPTInstaller.Converters;

public class StateSpinnerStateToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;
        
        if (value is not StatusSpinner.SpinnerState state)
            return null;
        
        switch (state)
        {
            case StatusSpinner.SpinnerState.Pending:
                return new SolidColorBrush(Colors.Gray);
            case StatusSpinner.SpinnerState.Running:
                return new SolidColorBrush(Colors.DodgerBlue);
            case StatusSpinner.SpinnerState.OK:
                return new SolidColorBrush(Colors.ForestGreen);
            case StatusSpinner.SpinnerState.Warning:
                return new SolidColorBrush(Colors.Goldenrod);
            case StatusSpinner.SpinnerState.Error:
                return new SolidColorBrush(Colors.Crimson);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value;
    }
}