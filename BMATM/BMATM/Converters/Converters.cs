using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BMATM.Converters
{
    /// <summary>
    /// Converts null values to Visibility enum values
    /// </summary>
    public class NullToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets whether null values should be visible or collapsed
        /// </summary>
        public bool NullIsVisible { get; set; } = false;

        /// <summary>
        /// Converts a value to Visibility
        /// </summary>
        /// <param name="value">The value to convert</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Visibility value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isNull = value == null;

            if (NullIsVisible)
            {
                return isNull ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                return isNull ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        /// Converts back from Visibility to the original value (not implemented)
        /// </summary>
        /// <param name="value">The value to convert back</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Converted value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported for NullToVisibilityConverter");
        }
    }

    /// <summary>
    /// Inverts boolean values
    /// </summary>
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse
        /// </summary>
        /// <param name="value">The boolean value to convert</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Inverted boolean value</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return false;
        }

        /// <summary>
        /// Converts back from inverted boolean to original value
        /// </summary>
        /// <param name="value">The inverted boolean value</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Original boolean value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }

            return true;
        }
    }

    /// <summary>
    /// Converts decimal values to currency strings
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the currency symbol
        /// </summary>
        public string CurrencySymbol { get; set; } = "EGP";

        /// <summary>
        /// Gets or sets whether to show zero values
        /// </summary>
        public bool ShowZeroValues { get; set; } = true;

        /// <summary>
        /// Converts a decimal value to currency string
        /// </summary>
        /// <param name="value">The decimal value to convert</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Formatted currency string</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return ShowZeroValues ? $"0.00 {CurrencySymbol}" : "";

            if (value is decimal decimalValue)
            {
                if (!ShowZeroValues && decimalValue == 0)
                    return "";

                return $"{decimalValue:N2} {CurrencySymbol}";
            }

            if (value is double doubleValue)
            {
                if (!ShowZeroValues && doubleValue == 0)
                    return "";

                return $"{doubleValue:N2} {CurrencySymbol}";
            }

            if (decimal.TryParse(value.ToString(), out decimal parsedValue))
            {
                if (!ShowZeroValues && parsedValue == 0)
                    return "";

                return $"{parsedValue:N2} {CurrencySymbol}";
            }

            return ShowZeroValues ? $"0.00 {CurrencySymbol}" : "";
        }

        /// <summary>
        /// Converts back from currency string to decimal value
        /// </summary>
        /// <param name="value">The currency string</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Decimal value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Remove currency symbol and parse
                var cleanValue = stringValue.Replace(CurrencySymbol, "").Trim();

                if (decimal.TryParse(cleanValue, out decimal result))
                {
                    return result;
                }
            }

            return 0m;
        }
    }

    /// <summary>
    /// Converts DateTime values to formatted date strings
    /// </summary>
    public class DateTimeConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the date format string
        /// </summary>
        public string DateFormat { get; set; } = "dd/MM/yyyy";

        /// <summary>
        /// Converts a DateTime value to formatted string
        /// </summary>
        /// <param name="value">The DateTime value to convert</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter (can override DateFormat)</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Formatted date string</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "";

            if (value is DateTime dateTime)
            {
                var format = parameter as string ?? DateFormat;
                return dateTime.ToString(format, culture);
            }

            //if (value is DateTime ? nullableDateTime && nullableDateTime.HasValue)
            //{
            //    var format = parameter as string ?? DateFormat;
            //    return nullableDateTime.Value.ToString(format, culture);
            //}

            return "";
        }

        /// <summary>
        /// Converts back from formatted string to DateTime value
        /// </summary>
        /// <param name="value">The formatted date string</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>DateTime value</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
            {
                if (DateTime.TryParse(stringValue, culture, DateTimeStyles.None, out DateTime result))
                {
                    return result;
                }
            }

            return targetType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;
        }
    }

    /// <summary>
    /// Converts reconciliation status to appropriate colors
    /// </summary>
    public class ReconciliationStatusToBrushConverter : IValueConverter
    {
        /// <summary>
        /// Converts reconciliation status to color brush
        /// </summary>
        /// <param name="value">The reconciliation status</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Color brush for the status</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                switch (status.ToLower())
                {
                    case "balanced":
                        return Application.Current.FindResource("SuccessBrush");
                    case "shortage":
                    case "over":
                        return Application.Current.FindResource("ErrorBrush");
                    case "pending":
                        return Application.Current.FindResource("WarningBrush");
                    default:
                        return Application.Current.FindResource("PrimaryBrush");
                }
            }

            return Application.Current.FindResource("PrimaryBrush");
        }

        /// <summary>
        /// Converts back from color brush to status (not implemented)
        /// </summary>
        /// <param name="value">The color brush</param>
        /// <param name="targetType">The target type</param>
        /// <param name="parameter">Converter parameter</param>
        /// <param name="culture">Culture info</param>
        /// <returns>Reconciliation status</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack is not supported for ReconciliationStatusToBrushConverter");
        }
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public static readonly BooleanToVisibilityConverter Instance = new BooleanToVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }
}
