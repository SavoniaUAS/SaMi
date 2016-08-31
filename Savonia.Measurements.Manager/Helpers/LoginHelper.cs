using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Routing;

namespace Savonia.Measurements.Manager.Helpers
{
    /// <summary>
    /// Class is used to create authentication information and authenticate user on controller level
    /// </summary>
    public class LoginHelper
    {
        /// <summary>
        /// Creates necessary login credentials which will be used to user authentication.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>

        public void CreateLoginCredentials(HttpContextBase context, string key)
        {
            //TODO: this class becomes obsolete, when system starts to use forms authentication
            context.Session.Clear();
            Guid quid = Guid.NewGuid();
            string kryptoQuid = Savonia.Measurements.Database.Helpers.DBHelper.Encrypt(quid.ToString());
            context.Session.Add(quid.ToString(), key);
        //TODO:Could formsauthenticationTicket used here?
       // https://msdn.microsoft.com/en-us/library/system.web.security.formsauthenticationticket%28v=vs.110%29.aspx       
            HttpCookie token = new HttpCookie("token");
            token.Value = kryptoQuid;
            token.HttpOnly = true;
            context.Response.Cookies.Add(token);
            token.Expires = DateTime.Now.AddHours(1);
        }

        /// <summary>
        /// Tries to returns login key using specified httpcontextbase.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetAuthenticationKey(HttpContextBase context)
        {
            string key = null;
            if (context.Request.Cookies.AllKeys.Contains("token"))
            {
                string token = context.Request.Cookies["token"].Value;
                if (token != null)
                {
                    string dekryptedToken = Database.Helpers.DBHelper.Decrypt(token);
                    key = (string)context.Session[dekryptedToken];
                }
            }

            return key;
        }

    }
}