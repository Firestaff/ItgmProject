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
                    return "";

                case WebExceptionStatus.TrustFailure:
                    return "Неверный логин или пароль!";

                case WebExceptionStatus.ConnectFailure:
                    return "Сервер не отвечает :(";

                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return HttpStatusCode.OK;
        }
    }
}
