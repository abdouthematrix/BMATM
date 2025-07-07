namespace BMATM.Converters;
/// <summary>
/// Converts boolean IsActive status to display text (Active/Inactive)
/// Optional parameter: "custom" to use custom text (e.g., "Online|Offline")
/// </summary>
public class BooleanToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isActive)
        {
            // Check for custom text parameter
            if (parameter is string customText && customText.Contains('|'))
            {
                var parts = customText.Split('|');
                return isActive ? parts[0] : parts[1];
            }

            return isActive ? "Active" : "Inactive";
        }
        return "Unknown";
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
}