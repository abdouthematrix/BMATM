namespace BMATM.Converters;

// ATM Type to Image Converter
public class ATMTypeToImageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ATMType atmType)
        {
            return atmType.GetImagePath();
        }

        // Fallback: if it's a string, try to parse it
        if (value is string atmTypeString && Enum.TryParse<ATMType>(atmTypeString, true, out var parsedType))
        {
            return parsedType.GetImagePath();
        }

        // Default fallback image
        return "pack://application:,,,/Resources/Images/bigatm.png";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
