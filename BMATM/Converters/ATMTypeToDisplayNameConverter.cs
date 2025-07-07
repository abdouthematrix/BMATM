namespace BMATM.Converters;

/// <summary>
/// Converts ATM type enum/string to user-friendly display name
/// </summary>
public class ATMTypeToDisplayNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ATMType atmType)
        {
            return atmType.GetDisplayName();
        }

        // Fallback: if it's a string, try to parse it
        if (value is string atmTypeString && Enum.TryParse<ATMType>(atmTypeString, true, out var parsedType))
        {
            return parsedType.GetDisplayName();
        }

        // Default fallback image
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
