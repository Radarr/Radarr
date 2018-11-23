using System;
using System.Globalization;

namespace NzbDrone.Common.Extensions
{
    public static class TryParseExtensions
    {
        public static int? ParseInt32(this string source)
        {
            int result = 0;

            if (int.TryParse(source, out result))
            {
                return result;
            }

            return null;
        }

        public static Nullable<long> ParseInt64(this string source)
        {
            long result = 0;

            if (long.TryParse(source, out result))
            {
                return result;
            }

            return null;
        }

        public static double? ParseDouble(this string source)
        {
            double result;

            if (double.TryParse(source.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }

            return null;
        }
    }
}
