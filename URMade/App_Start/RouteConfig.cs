using System.Web.Mvc;
using System.Web.Routing;

namespace URMade.App_Start
{
    public class RouteConfig
    {
        public static void Configure(RouteCollection routes)
        {
            routes.MapRoute(
                name: "ArtistWebsite",
                url: "artists/{slug}",
                defaults: new { controller = "Artist", action = "Website" }
            );
            routes.MapRoute(
                name: "UserProfile",
                url: "users/{slug}",
                defaults: new { controller = "User", action = "Details" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}