using URMade.Infrastructure;

namespace URMade.App_Start
{
    public class FilterConfig
    {
        public static void Configure(System.Web.Mvc.GlobalFilterCollection filters)
        {
            filters.Add(new RequreSecureConnectionFilter());
        }
    }
}
