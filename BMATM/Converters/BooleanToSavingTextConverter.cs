namespace BMATM.Converters;
/// <summary>
/// Converts boolean IsSaving to appropriate button text (Save / Saving...)
/// </summary>
public class BooleanToSavingTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isSaving)
        {
            return isSaving ? "Saving..." : "Save";
        }
        return "Save";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
