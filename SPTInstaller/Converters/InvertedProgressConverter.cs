using System.Globalization;
using Avalonia.Data.Converters;

namespace SPTInstaller.Converters;

public class InvertedProgressConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int progress)
        {
            return 100 - progress;
        }

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int invertedProgress)
        {
            return 100 - invertedProgress;
        }

        return value;
    }
}