using System;
namespace TauManager.Utils
{
    public static class GCT
    {
        public static DateTime? ParseGCTDate(string gct)
        {
            var pattern = "(\\d{3}.\\d{2})/";
            var GCTRegex = new System.Text.RegularExpressions.Regex(pattern);
            var match = GCTRegex.Match(gct);
            if (match.Success)
            {
                var dayFromStart = (int)Math.Round(decimal.Parse(match.Groups[1].Value) * 100);
                var UTCDate = new DateTime(1964, 01, 22).AddDays(dayFromStart);
                return UTCDate;
            }
            return null;
        }

        public static DateTime? ParseGCTDateExact(string gct)
        {
            var pattern = "(\\d{3}.\\d{2})/(\\d{2}):(\\d{3})";
            var GCTRegex = new System.Text.RegularExpressions.Regex(pattern);
            var match = GCTRegex.Match(gct);
            if (match.Success)
            {
                var dayFromStart = double.Parse(match.Groups[1].Value) * 100 +
                    double.Parse(match.Groups[2].Value) * 0.01 +
                    double.Parse(match.Groups[3].Value) * (double)0.00001;
                var UTCDate = new DateTime(1964, 01, 22).AddDays(dayFromStart);
                return UTCDate;
            }
            return null;
        }

        public static string MakeGCTDateTime(DateTime dt)
        {
            return (dt.Subtract(new DateTime(1964, 01, 22)).Days/100.0).ToString("0.00'/'") +
                    (dt.TimeOfDay.TotalDays*100).ToString("00.000").Replace('.', ':');
        }
    }
}