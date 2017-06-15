using URMade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace URMade
{
    public static class FilterAttributeUtilities
    {
        public static void RequirePermission(Permission permission, ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.GetCustomAttributes(typeof(OverrideDefaultPermissionAttribute), false).Any()) return;

            if (!SecurityHelper.LoggedInUserHas(permission))
            {
                filterContext.Result = new RedirectToRouteResult("Default", new System.Web.Routing.RouteValueDictionary(new
                {
                    controller = "Home",
                    action = "Index"
                }));
            }
        }
    }

    public class OverrideDefaultPermissionAttribute : ActionFilterAttribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class RequirePermissionAttribute : ActionFilterAttribute
    {
        public Permission Permission { get; set; }

        public RequirePermissionAttribute(Permission permission)
        {
            Permission = permission;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            FilterAttributeUtilities.RequirePermission(Permission, filterContext);
        }
    }
}