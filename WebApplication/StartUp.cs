using Microsoft.Owin;
using Owin;
using WebApplication;

[assembly: OwinStartup(typeof(StartUp))]
namespace WebApplication
{
    public partial class StartUp
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}