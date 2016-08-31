using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Diagnostics;

namespace Savonia.Measurements.Manager
{
    public class MvcApplication : System.Web.HttpApplication
    {
        /// <summary>
        /// Admins that have access to Manage section.
        /// </summary>
        public static List<string> AdminUsers = new List<string>();
        public static string MainSiteUrl = "~/";

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            var raw = Savonia.Measurements.Common.GlobalSettings.GetAdminUsersRaw;
            if (raw != null)
            {
                var admins = raw.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (var a in admins)
                {
                    AdminUsers.Add(a.Trim());
                }
            }
            MvcApplication.MainSiteUrl = Savonia.Measurements.Common.GlobalSettings.GetMainSiteUrl;
        }

        public void Session_OnStart()
        {

        }
        public void Application_BeginRequest(Object source, EventArgs e)
        {
        }
        public void Session_OnEnd()
        {
            try
            {
                List<string> paths = new List<string>();
                paths.Add(System.Web.Hosting.HostingEnvironment.MapPath(Common.GlobalSettings.GetCsvPath));
                paths.Add(System.Web.Hosting.HostingEnvironment.MapPath(Common.GlobalSettings.GetJSONPath));
                //TODO: Make this better!
                string fullpath = "";
                string filename = "";
                int i = 0;
                foreach (var p in paths)
                {
                    switch (i)
                    {
                        case 1:
                            filename = Session.SessionID + ".json";
                            break;
                        default:
                            filename = Session.SessionID + ".csv";
                            break;
                    }
                    fullpath = System.IO.Path.Combine(p, filename);
                    if (System.IO.Directory.Exists(p))
                    {
                        if (System.IO.File.Exists(fullpath))
                        {
                            System.IO.File.Delete(fullpath);
                        }
                    }
                    i++;
                }
            }
            catch (Exception e)
            {

                Trace.WriteLine(e.Message);
                //TODO: should we use the error logger here?

                //System.Web.HttpContext.Current.Request

                // Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(e, new HttpRequest("", "", ""));
            }
            finally
            {
                Session.Clear();
            }
        }


    }
}
