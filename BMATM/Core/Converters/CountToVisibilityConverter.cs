namespace BMATM.Converters;

/// <summary>
/// Unified converter for count/number to Visibility
/// Parameters:
/// - Number only (e.g., "5") = threshold value (default 0)
/// - "inverse" or "!" = inverts the visibility logic
/// - "5,inverse" = uses threshold 5 and inverts logic
/// 
/// Normal: count <= threshold = Visible, count > threshold = Collapsed
/// Inverse: count <= threshold = Collapsed, count > threshold = Visible
/// </summary>
public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return GetDefaultVisibility(parameter);

        if (!int.TryParse(value.ToString(), out int count))
            return GetDefaultVisibility(parameter);

        var (threshold, shouldInvert) = ParseParameter(parameter);

        bool isVisible = count <= threshold;

        if (shouldInvert)
            isVisible = !isVisible;

        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("UnifiedCountToVisibilityConverter is a one-way converter.");
    }

    private (int threshold, bool shouldInvert) ParseParameter(object parameter)
    {
        var paramStr = parameter?.ToString()?.ToLower() ?? "";
        var shouldInvert = paramStr.Contains("inverse") || paramStr.Contains("!");

        // Extract threshold value
        var parts = paramStr.Split(',');
        int threshold = 0;

        foreach (var part in parts)
        {
            var cleanPart = part.Trim().Replace("inverse", "").Replace("!", "");
            if (int.TryParse(cleanPart, out int value))
            {
                threshold = value;
                break;
            }
        }

        return (threshold, shouldInvert);
    }

    private Visibility GetDefaultVisibility(object parameter)
    {
        var (_, shouldInvert) = ParseParameter(parameter);
        // Default for null value: show if not inverted, hide if inverted
        return shouldInvert ? Visibility.Collapsed : Visibility.Visible;
    }
}
