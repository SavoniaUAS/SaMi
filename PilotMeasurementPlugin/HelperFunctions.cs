using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PilotMeasurementPlugin
{
    public static class HelperFunctions
    {
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
