using System;

namespace DatadogTakeHome.Core
{
    public class DateFormatter
    {
        public static string FormatDate(long epochTimestampSecond)
        {
            return DateTimeOffset.FromUnixTimeSeconds(epochTimestampSecond).ToString("u");
        }
    }
}
