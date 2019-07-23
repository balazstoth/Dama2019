using Autofac;
using Autofac.Integration.Mvc;
using Dama.Data.Interfaces;
using Dama.Data.Models;
using Dama.Data.Sql.Interfaces;
using Dama.Data.Sql.Repositories;
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

            builder.RegisterType<UserSqlRepository>().As<IUserRepository>();
            builder.RegisterType<ContentRepository>().As<IContentRepository>();
            builder.RegisterType<FixedActivitySqlRepository>().As<IRepository<FixedActivity>>();
            builder.RegisterType<UnfixedActivitySqlRepository>().As<IRepository<UnfixedActivity>>();
            builder.RegisterType<UndefinedActivitySqlRepository>().As<IRepository<UndefinedActivity>>();
            builder.RegisterType<DeadlineActivitySqlRepository>().As<IRepository<DeadlineActivity>>();
            builder.RegisterType<LabelSqlRepository>().As<IRepository<Label>>();
            builder.RegisterType<CategorySqlRepository>().As<IRepository<Category>>();
            builder.RegisterType<MilestoneSqlRepository>().As<IRepository<Milestone>>();
            builder.RegisterType<RepositorySettings>().As<IRepositorySettings>();
            builder.RegisterType<RepositoryManager>().As<IRepositoryManager>();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}
