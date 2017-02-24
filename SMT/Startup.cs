using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SMT.Startup))]
namespace SMT
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
