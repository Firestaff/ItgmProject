using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace Itgm.Converters
{
    /// <summary>
    /// Преобразует <see cref="WebExceptionStatus"/> в текстовое представление.
    /// </summary>
    public class ConnectionStateAuthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (WebExceptionStatus)value;

            switch (state)
            {
                case WebExceptionStatus.Success:
                    return "Введите PinCode";

                case WebExceptionStatus.TrustFailure:
                    return "Введен неверный PinCode!";

                case WebExceptionStatus.ConnectFailure:
                    return "Сервер не отвечает :(";

                default:
                    return "Введите PinCode";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return HttpStatusCode.OK;
        }
    }
}
