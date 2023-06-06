using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace RenameMe.Api.Engines.Bases
{
    public static class EngineHub
    {
        public static WebApplication BuildWithEngines(this WebApplicationBuilder builder)
        {
            WebApplication? app = null;
            builder.RegisterEngines();
            builder.StartBuilderEngines();
            builder.Services.AddSingleton(services =>
            {
                app ??= builder.Build();
                return app;
            });
            app = builder.Build();
            app.StartAppEngines();
            return app;
        }

        private static void RegisterEngines(this WebApplicationBuilder builder)
        {
            var iengineType = typeof(IEngine);
            var engineTypes = iengineType
                  .Assembly
                  .ExportedTypes
                  .Where(e => e.GetInterfaces().Contains(iengineType) && e.IsClass && !e.IsAbstract)
                  .ToList();

            foreach (var engineType in engineTypes)
            {
                var itype = engineType.GetInterfaces().FirstOrDefault(e => e.GetInterfaces().Contains(iengineType) && e != iengineType);
                if (itype != null)
                {
                    builder.Services.AddSingleton(itype, engineType);
                }
            }
        }

        private static void StartBuilderEngines(this WebApplicationBuilder builder)
        {
            builder.Host.UseServiceProviderFactory(new CustomAutofacServiceProviderFactory())
                   .ConfigureServices((_, serviceBuilder) =>
                   {
                       serviceBuilder.AddSingleton(serviceBuilder);
                   })
                   .ConfigureContainer<ContainerBuilder>((_, container) =>
                   {
                       builder.Services.AddSingleton(container);
                       var services = builder.Services.BuildServiceProvider();
                       var engines = services.GetServices<IBuilderEngine>();
                       if (engines != null)
                       {
                           foreach (var engine in engines)
                           {
                               engine.Run();
                           }
                       }
                       container.Populate(builder.Services);
                   });
        }

        private static void StartAppEngines(this WebApplication app)
        {
            var engines = app.Services.GetServices<IAppEngine>();
            if (engines != null)
            {
                foreach (var engine in engines)
                {
                    engine.Run();
                }
            }
        }
    }
}
