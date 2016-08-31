using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Savonia.Measurements.Manager.Helpers
{
    public static class ImageHelper
    {
        public static string GetBase64Encoded(this byte[] data, string appendPrefix = "")
        {
            if (null != data)
            {
                return string.Format("{0}{1}", appendPrefix, System.Convert.ToBase64String(data));
            }
            return string.Empty;
        }
    }
}