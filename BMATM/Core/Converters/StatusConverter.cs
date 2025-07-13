namespace BMATM.Converters;

/// <summary>
/// Unified converter for boolean status to text or color
/// Parameters:
/// - Text: "Active|Inactive" or "Online|Offline" for custom status text
/// - Color: "#00FF00|#FF0000" for custom colors (hex format)
/// - "color" = use default colors (Green/Red)
/// - "color,#00FF00|#FF0000" = use custom colors
/// </summary>
public class StatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (!(value is bool isActive))
            return GetDefaultValue(parameter);

        var paramStr = parameter?.ToString() ?? "";
        var shouldReturnColor = paramStr.ToLower().Contains("color") || paramStr.Contains("#");

        if (shouldReturnColor)
        {
            return ConvertToColor(isActive, paramStr);
        }
        else
        {
            return ConvertToText(isActive, paramStr);
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string status)
        {
            return status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Online", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    private object ConvertToText(bool isActive, string parameter)
    {
        // Check for custom text parameter
        var textParam = parameter;
        if (parameter.ToLower().Contains("color"))
        {
            // Extract non-color part if mixed parameter
            var parts = parameter.Split(',');
            textParam = parts.FirstOrDefault(p => !p.ToLower().Contains("color") && !p.Contains("#")) ?? "";
        }

        if (!string.IsNullOrEmpty(textParam) && textParam.Contains('|'))
        {
            var parts = textParam.Split('|');
            return isActive ? parts[0] : parts[1];
        }

        return isActive ? "Active" : "Inactive";
    }

    private object ConvertToColor(bool isActive, string parameter)
    {
        // Look for hex color pattern
        var colorParam = parameter;
        if (parameter.Contains(','))
        {
            // Extract color part from mixed parameter
            var parts = parameter.Split(',');
            colorParam = parts.FirstOrDefault(p => p.Contains("#")) ?? "";
        }

        if (!string.IsNullOrEmpty(colorParam) && colorParam.Contains('|'))
        {
            var parts = colorParam.Split('|');
            try
            {
                var activeColor = (Color)ColorConverter.ConvertFromString(parts[0]);
                var inactiveColor = (Color)ColorConverter.ConvertFromString(parts[1]);
                return isActive ?
                    new SolidColorBrush(activeColor) :
                    new SolidColorBrush(inactiveColor);
            }
            catch
            {
                // Fall back to default colors if parsing fails
            }
        }

        // Default colors
        return isActive ?
            new SolidColorBrush(Colors.Green) :
            new SolidColorBrush(Colors.Red);
    }

    private object GetDefaultValue(object parameter)
    {
        var paramStr = parameter?.ToString()?.ToLower() ?? "";

        if (paramStr.Contains("color") || paramStr.Contains("#"))
            return new SolidColorBrush(Colors.Gray);

        return "Unknown";
    }
}