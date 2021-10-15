using System;

namespace DatadogTakeHome.Core
{
    public class DateFormatter
    {
        /// <summary>
        /// Formats epoch time into a universal format to be consistent across OS & developer machines.
        /// </summary>
        /// <param name="epochTimestampSecond"></param>
        /// <returns></returns>
        public static string FormatDate(long epochTimestampSecond)
        {
            return DateTimeOffset.FromUnixTimeSeconds(epochTimestampSecond).ToString("u");
        }
    }
}
