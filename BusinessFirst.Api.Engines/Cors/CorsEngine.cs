using RenameMe.Api.Infrastructure.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RenameMe.Api.Engines.Cors
{
    public class ConfigureCors //: IBuilderEngine
    {
        private readonly IServiceCollection services;

        public ConfigureCors(IServiceCollection serviceDescriptors)
        {
            this.services = serviceDescriptors;
        }
        public void Run()
        {
            services.AddCors(options =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var corSetting = serviceProvider.GetService<CorsSetting>()!;
                var environment = serviceProvider.GetService<IWebHostEnvironment>();
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyHeader()
                          .AllowAnyMethod();
                    if (environment != null && environment.IsDevelopment())
                    {
                        policy.AllowAnyOrigin();
                    }
                    else
                    {
                        if (corSetting.Origins != null)
                        {
                            policy.WithOrigins(corSetting.Origins)
                                  .AllowCredentials();
                        }
                    }
                });
            });
        }
    }

    public class UseCors //: IAppEngine
    {
        private readonly WebApplication webApplication;

        public UseCors(WebApplication webApplication)
        {
            this.webApplication = webApplication;
        }
        public void Run()
        {
            webApplication.UseCors();
        }
    }
}
