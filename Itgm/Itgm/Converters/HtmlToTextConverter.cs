using System;
using System.Globalization;
using System.Windows.Data;

namespace Itgm.Converters
{
    /// <summary>
    /// Удаляет из входящей строки все ссылки.
    /// </summary>
    public class HtmlToTextConverter : IValueConverter
    {
        private const string SUBSTRING = "http";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value.ToString();

            var openIndex = text.IndexOf(SUBSTRING);

            if (openIndex != -1)
            {
                text = text.Substring(0,openIndex);
            }

            return text.Trim();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
