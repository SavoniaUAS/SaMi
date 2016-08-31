using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Savonia.Measurements.Models.Helpers
{
    public static class QueryStringHelper
    {
        public static DateTimeOffset? QueryParamToDateTimeOffset(this string text)
        {
            DateTimeOffset? model = null;
            if (!string.IsNullOrWhiteSpace(text))
            {
                DateTimeOffset d;
                if (DateTimeOffset.TryParse(text, out d))
                {
                    model = d;
                }
            }

            return model;
        }

        public static int? QueryParamToInt(this string text)
        {
            int? model = null;
            if (!string.IsNullOrWhiteSpace(text))
            {
                int i;
                if (int.TryParse(text, out i))
                {
                    model = i;
                }
            }
            return model;
        }

        public static bool QueryParamToBoolean(this string text, bool defaultValue)
        {
            bool model = defaultValue;
            if (!string.IsNullOrWhiteSpace(text))
            {
                bool b;
                if (bool.TryParse(text, out b))
                {
                    model = b;
                }
                else
                {
                    if ("1" == text)
                    {
                        model = true;
                    }
                    else if ("0" == text)
                    {
                        model = false;
                    }
                }
            }
            return model;
        }
    }
}