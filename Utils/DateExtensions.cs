using System;

namespace TauManager.Utils
{
    public static class DateExtensions
    {
        public static DateTime EnsureUTC(this DateTime sourceDate)
        {
            return new DateTime(
                    sourceDate.Year,
                    sourceDate.Month,
                    sourceDate.Day,
                    sourceDate.Hour,
                    sourceDate.Minute,
                    sourceDate.Second,
                    DateTimeKind.Utc
                );
        }
    }
}