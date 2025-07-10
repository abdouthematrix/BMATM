namespace BMATM.Converters;
/// <summary>
/// Converts boolean IsActive status to color brush (Green for Active, Red for Inactive)
/// Optional parameter: hex colors like "#00FF00|#FF0000" for custom colors
/// </summary>
public class BooleanToStatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            // Check for custom colors parameter
            if (parameter is string colorParam && colorParam.Contains('|'))
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

            return isActive ?
                new SolidColorBrush(Colors.Green) :
                new SolidColorBrush(Colors.Red);
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("BooleanToStatusColorConverter is a one-way converter.");
    }
}