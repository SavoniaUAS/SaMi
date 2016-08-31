using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Savonia.Measurements.Manager.Helpers
{
    public static class UrlHelperExtensions
    {
        public static string ContentAbsUrl(this UrlHelper url, string relativeContentPath)
        {
            Uri u;
            if (Uri.TryCreate(relativeContentPath, UriKind.Absolute, out u))
            {
                return relativeContentPath;
            }

            Uri contextUri = HttpContext.Current.Request.Url;

            var baseUri = string.Format("{0}://{1}{2}", contextUri.Scheme,
               contextUri.Host, contextUri.Port == 80 || contextUri.Port == 443 ? string.Empty : ":" + contextUri.Port);
            return string.Format("{0}{1}", baseUri, VirtualPathUtility.ToAbsolute(relativeContentPath));
        }
    }
}