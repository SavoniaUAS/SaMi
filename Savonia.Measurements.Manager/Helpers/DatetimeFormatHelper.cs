using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace Savonia.Measurements.Manager.Helpers
{
    public static class DatetimeFormatHelper
    {
        public static List<string> GetPossibleDatetimeFormats()
        {
            CultureInfo ci = CultureInfo.GetCultureInfo("en-US");
            List<string> formats = new List<string>();
            formats.Add("yyyy-MM-ddTHH\\:mm\\:ss.zzz");
            formats.AddRange(ci.DateTimeFormat.GetAllDateTimePatterns()); ;

            return formats;
        }
    }
}