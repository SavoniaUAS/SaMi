using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;

namespace Savonia.Measurements.Manager.Models
{
    public class SamiAuthorizeAttribute : AuthorizeAttribute
    {
        public AccessControls Permission { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            var userAccessControl = GetUserAccessControl(httpContext.User);

            return (Permission & userAccessControl) == Permission;
        }

        public static AccessControls GetUserAccessControl(IPrincipal user)
        {
            AccessControls roles = AccessControls.None;

            foreach (var role in MvcApplication.AdminUsers)
            {
                if (user.IsInRole(role))
                {
                    roles = roles | AccessControls.Admin;
                    break;
                }
            }

            return roles;
        }
    }
}
