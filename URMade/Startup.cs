using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(URMade.Startup))]
namespace URMade
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
