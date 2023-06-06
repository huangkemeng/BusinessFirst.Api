using Autofac;
using RenameMe.Api.Engines.Bases;
using RenameMe.Api.Infrastructure.Bases;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace RenameMe.Api.Engines.Setting
{
    public class ConfigureSetting : IBuilderEngine
    {
        private readonly IServiceCollection services;

        public ConfigureSetting(IServiceCollection services)
        {
            this.services = services;
        }
        public void Run()
        {
            var isettingType = typeof(ISetting);
            var infrastructureAssembly = isettingType.Assembly;
            var settingTypes = infrastructureAssembly
                ?.ExportedTypes
                .Where(e => e.GetInterfaces().Contains(isettingType) && e.IsClass && !e.IsAbstract)
                .ToArray();
            var basePath = AppContext.BaseDirectory;
            IConfigurationBuilder appsettingConfigurationBuilder = new ConfigurationBuilder();
            if (settingTypes != null)
            {
                foreach (var settingType in settingTypes)
                {
                    var setting = Activator.CreateInstance(settingType)!;
                    var settingName = settingType.Name;
                    if (setting is IJsonFileSetting jsonFileSetting)
                    {
                        var path = Path.Combine(basePath, jsonFileSetting.JsonFilePath);
                        if (!string.IsNullOrWhiteSpace(jsonFileSetting.JsonFilePath)
                          && File.Exists(path))
                        {
                            appsettingConfigurationBuilder.AddJsonFile(path, false);
                        }
                    }
                    else if (setting is IStringSetting stringSetting)
                    {
                        services.AddSingleton(settingType, stringSetting);
                    }
                }
            }
            if (File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                appsettingConfigurationBuilder
                    .AddJsonFile(Path.Combine(basePath, "appsettings.json"), false);
            };
            var configuration = appsettingConfigurationBuilder
                                .AddEnvironmentVariables()
                                .Build();
            services.AddSingleton(configuration);
            services.AddSingleton<IConfiguration>(configuration);
            if (settingTypes != null)
            {
                foreach (var settingType in settingTypes)
                {
                    var setting = Activator.CreateInstance(settingType)!;
                    var settingName = settingType.Name;
                    configuration.GetSection(settingName).Bind(setting);
                    services.AddSingleton(settingType, setting);
                }
            }
        }
    }
}
