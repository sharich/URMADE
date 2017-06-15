using URMade.App_Start;
using System;
using System.Diagnostics;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Optimization;

namespace URMade
{

    public class Global : System.Web.HttpApplication
    {
        public override string GetVaryByCustomString(System.Web.HttpContext context, string arg)
        {
            if (arg.Equals("loggedin", StringComparison.InvariantCultureIgnoreCase))
            {
                    return User.Identity.IsAuthenticated.ToString();
            }
            if (arg.Equals("MyArtist", StringComparison.InvariantCultureIgnoreCase))
            {
                Models.ApplicationUser user = SecurityHelper.GetLoggedInUser();

				if (user == null)
					return "public";

				string url = context.Request.RawUrl;

				if (url.Contains("/Artists/"))
				{
					string slug = url.Replace("/Artists/", "");

					if (user.IsMyArtists(slug))
						return user.Id;
				}
				else if (url.Contains("/Artist/Website/"))
				{
					int id;

					if (int.TryParse(url.Replace("Artist/Website/", ""), out id) && user.IsMyArtists(id))
						return user.Id;
				}
            }
            if (arg.Equals("User", StringComparison.InvariantCultureIgnoreCase))
            {
                    return context.User.Identity.Name;
            }
            return base.GetVaryByCustomString(context, arg);
        }


        protected void Application_Start(object sender, EventArgs e)
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.Configure(GlobalFilters.Filters);
            RouteConfig.Configure(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            InitializeAccountData.InitializeDatabase();
        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

            Exception ex = Server.GetLastError();
            if (ex != null)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}