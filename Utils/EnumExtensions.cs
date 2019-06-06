using System.Globalization;
using System.Linq;
using System;
using System.Collections.Generic;

namespace TauManager.Utils
{
    public static class EnumExtensions
    {
        public static Dictionary<T, string> ToDictionary<T>(Type targetEnumType)
        {
            var enumValues = Enum.GetValues(targetEnumType).Cast<IConvertible>().Select(v => (T)v.ToType(typeof(T), CultureInfo.CurrentCulture));
            var result = new Dictionary<T, string>();
            foreach(var value in enumValues)
            {
                result[value] = Enum.GetName(targetEnumType, value);
            }
            return result;
        }
    }
}