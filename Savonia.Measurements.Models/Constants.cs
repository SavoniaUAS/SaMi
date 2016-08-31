using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Models
{
    public static class Constants
    {
        public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:sszzz";
        public const string DateFormatForJavaScript = "yy-mm-dd";
        public const string DateFormatISO8601 = "yyyy-MM-dd";
        /// <summary>
        /// Default Take value. Equals 20.
        /// </summary>
        public const int DefaultTakeValue = 20;

        //http://www.w3.org/TR/NOTE-datetime
    }
}
