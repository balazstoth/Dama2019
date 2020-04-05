using Autofac;
using Autofac.Integration.Mvc;
using Dama.Data.Sql.Interfaces;
using Dama.Data.Sql.Repositories;
using Dama.Data.Sql.SQL;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Dama.Web
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            InjectDependency();
        }

        private void InjectDependency()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterSource(new ViewRegistrationSource());

            //builder.RegisterType<RepositorySettings>().As<IRepositorySettings>();
            //builder.RegisterType<UnitOfWork>().As<IUnitOfWork>();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
