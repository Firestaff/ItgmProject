using System;
using System.Globalization;

namespace InstaSharper.Helpers
{
    public static class DateTimeHelper
    {
        public static DateTime UnixTimestampMilisecondsToDateTime(string unixTime)
        {
            var time = Convert.ToDouble(unixTime) / 1000000;
            return FromUnixSeconds(time.ToString());
        }

        public static DateTime FromUnixSeconds(string unixTime)
        {
            try
            {
                var time = Convert.ToDouble(unixTime?.Replace(',', '.'), CultureInfo.GetCultureInfo("en-US"));
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(time);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }
    }
}