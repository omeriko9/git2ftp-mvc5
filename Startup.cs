using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(git2ftp_mvc5.Startup))]
namespace git2ftp_mvc5
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
