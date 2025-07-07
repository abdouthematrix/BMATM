namespace BMATM.Converters;
/// <summary>
/// Converts boolean IsEditMode to appropriate title text (Add ATM / Edit ATM)
/// </summary>
public class BooleanToAddEditTitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isEditMode)
        {
            return isEditMode ? "Edit ATM" : "Add ATM";
        }
        return "Add ATM";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
