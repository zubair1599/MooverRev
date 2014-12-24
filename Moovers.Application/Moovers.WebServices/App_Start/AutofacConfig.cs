using System.Reflection;
using Autofac;
using Autofac.Integration.WebApi;
using Business.Interfaces;
using Business.Repository;
using Business.Repository.Models;
using Moovers.Webservices;
using Moovers.Webservices.Services;
using Moovers.WebServices.Services;
using Moovers.WebServices.Services.Concrete;

namespace Moovers.WebServices
{
    public static class AutofacConfig
    {
        public static AutofacWebApiDependencyResolver RegisterDependencies()
        {

            var builder = new ContainerBuilder();

            // Register the Web API controllers.
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();

            // Register dependencies.
            builder.RegisterType<HmacSignatureCalculator>().As<ISignatureCalculator>();
            builder.RegisterType<CanonicalRepresentationBuilder>().As<IMessageRepresentationBuilder>();
            builder.RegisterType<EmployeeAuthenticationRepository>().As<ICustomAuthenticationRepository>();
            builder.RegisterType<CacheRepository>().As<ICacheRepository>();
            builder.RegisterType<MoverScheduleRepository>().As<IMoverScheduleRepository>();
            builder.RegisterType<GlobalConfigurationHelper>().As<IConfigurationHelper>();

            builder.Register(i => new ResponseContentMd5Handler()
            {
                // InnerHandler = new HttpControllerDispatcher(i.Resolve<IConfigurationHelper>().Configuration)
            });
            builder.Register(container => new TokenHandler(container.Resolve<ICustomAuthenticationRepository>(),
               container.Resolve<IMessageRepresentationBuilder>()));

            builder.Register(container => new HmacAuthenticationHandler
                (
                container.Resolve<ICustomAuthenticationRepository>(),
                container.Resolve<IMessageRepresentationBuilder>(),
                container.Resolve<ISignatureCalculator>()
                ));
           
            builder.Register(container => new RouteBuilder(container.Resolve<HmacAuthenticationHandler>()))
                .As<IRouteBuilder>();

            var resolver = new AutofacWebApiDependencyResolver(builder.Build());
            return resolver;
        }
    }
}
