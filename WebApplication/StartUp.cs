using Owin;
using Microsoft.Owin;

[assembly: OwinStartupAttribute(typeof(WebApplication.App_Start.StartUp))]
namespace WebApplication.App_Start
{
    public partial class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}