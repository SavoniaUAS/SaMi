using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Savonia.Measurements.Models.Helpers
{
    public static class StringHelper
    {
        public static DateTimeOffset ToDateTimeOffset(this string value)
        {
            DateTimeOffset o;
            if (!DateTimeOffset.TryParse(value, out o))
            {
                o = DateTimeOffset.Now;
            }

            return o;
        }

        public static long ToLong(this string value)
        {
            long i = 0;
            long.TryParse(value, out i);

            return i;
        }

        public static double ToDouble(this string value)
        {
            double d = double.NaN;
            if (null != value)
            {
                value = value.Replace(',', '.');
            }
            double.TryParse(value, System.Globalization.NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out d);

            return d;
        }



    }
}