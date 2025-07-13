namespace BMATM.Converters;

// <summary>
/// Unified converter for boolean values with multiple output options
/// Parameters:
/// - "inverse" or "!" = inverts the boolean
/// - "visibility" = converts to Visibility (Visible/Collapsed)
/// - "inverse,visibility" = inverts boolean and converts to Visibility
/// </summary>
public class BooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(value is bool boolValue))
            return GetDefaultValue(targetType, parameter);

        var paramStr = parameter?.ToString()?.ToLower() ?? "";
        var shouldInvert = paramStr.Contains("inverse") || paramStr.Contains("!");
        var shouldConvertToVisibility = paramStr.Contains("visibility");

        // Apply inversion if requested
        if (shouldInvert)
            boolValue = !boolValue;

        // Convert to visibility if requested
        if (shouldConvertToVisibility)
            return boolValue ? Visibility.Visible : Visibility.Collapsed;

        // Return boolean value
        return boolValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var paramStr = parameter?.ToString()?.ToLower() ?? "";
        var shouldInvert = paramStr.Contains("inverse") || paramStr.Contains("!");
        var shouldConvertToVisibility = paramStr.Contains("visibility");

        bool result;

        if (shouldConvertToVisibility && value is Visibility visibility)
        {
            result = visibility == Visibility.Visible;
        }
        else if (value is bool boolValue)
        {
            result = boolValue;
        }
        else
        {
            return false;
        }

        return shouldInvert ? !result : result;
    }

    private object GetDefaultValue(Type targetType, object parameter)
    {
        var paramStr = parameter?.ToString()?.ToLower() ?? "";

        if (paramStr.Contains("visibility"))
            return Visibility.Collapsed;

        return paramStr.Contains("inverse") || paramStr.Contains("!") ? true : false;
    }
}