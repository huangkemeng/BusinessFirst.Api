using Autofac;
using RenameMe.Api.Engines.Bases;
using RenameMe.Api.Realization.Bases;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace RenameMe.Api.Engines.ServiceRegister
{
    public class ScanServiceEngine : IBuilderEngine
    {
        private readonly IServiceCollection services;

        public ScanServiceEngine(IServiceCollection services)
        {
            this.services = services;
        }
        public void Run()
        {
            var irealizationType = typeof(IRealization);
            var types = irealizationType
                .Assembly
                .ExportedTypes
                .Where(e => e.GetCustomAttribute<AsTypeAttribute>() != null)
                .ToArray();

            foreach (var implementationType in types)
            {
                var asTypeFlag = implementationType.GetCustomAttribute<AsTypeAttribute>()!;
                if (asTypeFlag.Types != null && asTypeFlag.Types.Any())
                {
                    foreach (var baseType in asTypeFlag.Types)
                    {
                        AddService(asTypeFlag.Lifetime, baseType, implementationType);
                    }
                }
                else
                {
                    var baseTypes = implementationType.GetInterfaces();
                    if (baseTypes != null && baseTypes.Any())
                    {
                        foreach (var baseType in baseTypes)
                        {
                            AddService(asTypeFlag.Lifetime, baseType, implementationType);
                        }
                    }
                    if (implementationType.BaseType != null)
                    {
                        AddService(asTypeFlag.Lifetime, implementationType.BaseType, implementationType);
                    }
                    AddService(asTypeFlag.Lifetime, implementationType, implementationType);
                }
            }
        }

        private void AddService(LifetimeEnum lifetime, Type baseType, Type implementationType)
        {
            switch (lifetime)
            {
                case LifetimeEnum.SingleInstance:
                    services.AddSingleton(baseType, implementationType);
                    break;
                case LifetimeEnum.Transient:
                    services.AddTransient(baseType, implementationType);
                    break;
                case LifetimeEnum.Scope:
                    services.AddScoped(baseType, implementationType);
                    break;
            }
        }

    }
}
