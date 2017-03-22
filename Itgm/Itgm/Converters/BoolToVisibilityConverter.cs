using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Itgm.Converters
{
    /// <summary>
    /// Преобразует булевское значение в <see cref="Visibility"/> и обратно.
    /// True = Visible.
    /// False = Collapsed.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;

            if (boolValue)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Visibility)value;

            if (visibilityValue == Visibility.Collapsed ||
                visibilityValue == Visibility.Hidden)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
