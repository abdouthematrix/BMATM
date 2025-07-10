namespace BMATM.Converters;
/// <summary>
/// Converts a count/number to Visibility. 
/// Returns Visible when count is 0 (for empty states), Collapsed otherwise.
/// Optional parameter can specify threshold (e.g., "5" to show when count <= 5).
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return Visibility.Visible;

        // Try to convert to integer
        if (int.TryParse(value.ToString(), out int count))
        {
            // Check if parameter specifies a threshold
            int threshold = 0;
            if (parameter != null && int.TryParse(parameter.ToString(), out int paramValue))
            {
                threshold = paramValue;
            }

            // Show when count is <= threshold (default 0 for empty state)
            return count <= threshold ? Visibility.Visible : Visibility.Collapsed;
        }

        // If conversion fails, assume empty state
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("CountToVisibilityConverter is a one-way converter.");
    }
}
