using Dama.Data.Enums;
using Dama.Data.Models;
using Dama.Data.Sql.SQL;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(typeof(Dama.Web.App_Start.Startup))]

namespace Dama.Web.App_Start
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            Initialize();
        }

        private void ConfigureAuth(IAppBuilder app)
        {
            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login")
            });
        }

        private void Initialize()
        {
            using (var context = new DamaContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<User>(new UserStore<User>(context));

                if (!roleManager.RoleExists("SuperAdmin"))
                {
                    var newRole = new IdentityRole();
                    newRole.Name = "SuperAdmin";
                    roleManager.Create(newRole);
                }

                if (!roleManager.RoleExists("Admin"))
                {
                    var newRole = new IdentityRole();
                    newRole.Name = "Admin";
                    roleManager.Create(newRole);
                }

                if (!roleManager.RoleExists("SimpleUser"))
                {
                    var newRole = new IdentityRole();
                    newRole.Name = "SimpleUser";
                    roleManager.Create(newRole);
                }

                var superAdmin = new User()
                {
                    UserName = "superAdmin",
                    Password = "superAdmin",
                    Email = "superadmin@dama.com",
                    FirstName = "Super",
                    LastName = "Admin",
                };

                var result = userManager.Create(superAdmin, superAdmin.Password);

                if (result.Succeeded)
                {
                    userManager.AddToRole(superAdmin.Id, UserRole.SuperAdmin.ToString());
                    userManager.AddToRole(superAdmin.Id, UserRole.Admin.ToString());
                    userManager.AddToRole(superAdmin.Id, UserRole.SimpleUser.ToString());
                }
            }
        }
    }
}
