using RenameMe.Api.Engines.Bases;
using RenameMe.Api.Infrastructure.Bases;
using RenameMe.Api.Primary.Contracts.Bases;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.RegularExpressions;

namespace RenameMe.Api.Engines.Swagger
{
    public class ConfigureSwagger : IBuilderEngine
    {
        private readonly IServiceCollection services;

        public ConfigureSwagger(IServiceCollection services)
        {
            this.services = services;
        }
        public void Run()
        {
            services.AddSwaggerGen(options =>
            {
                typeof(SwaggerApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
                {
                    var info = f.GetCustomAttributes(typeof(SwaggerGroupInfoAttribute), false).OfType<SwaggerGroupInfoAttribute>().FirstOrDefault();
                    options.SwaggerDoc(f.Name, new OpenApiInfo
                    {
                        Title = info?.Title,
                        Version = info?.Version,
                        Description = info?.Description
                    });
                });

                options.SwaggerDoc("Other", new OpenApiInfo
                {
                    Title = "其他"
                });

                options.DocInclusionPredicate((docName, apiDescription) =>
                {
                    if (docName == "Other")
                    {
                        return string.IsNullOrEmpty(apiDescription.GroupName);
                    }
                    else
                    {
                        if (docName == apiDescription.GroupName)
                        {
                            return true;
                        }
                        else if (apiDescription.GroupName == "*")
                        {
                            if (Enum.TryParse(docName, out SwaggerApiGroupNames groupName))
                            {
                                var fieldInfo = typeof(SwaggerApiGroupNames).GetField(docName)!;
                                var info = fieldInfo.GetCustomAttributes(typeof(SwaggerGroupInfoAttribute), false).OfType<SwaggerGroupInfoAttribute>().FirstOrDefault();
                                if (info != null && info.MatchRule != null && apiDescription.RelativePath != null)
                                {
                                    var matched = new Regex(info.MatchRule).Match(apiDescription.RelativePath);
                                    return matched.Success;
                                }
                            }
                        }
                        return false;
                    }
                });
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      \r\n\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
                var basePath = AppContext.BaseDirectory;
                var apiAssembly = Assembly.GetEntryAssembly()!;
                var primaryAssembly = typeof(IContract<>).Assembly;
                var infrastructureAssembly = typeof(ISetting).Assembly;
                options.IncludeXmlComments(Path.Combine(basePath, $"{apiAssembly.GetName().Name}.xml"), true);
                options.IncludeXmlComments(Path.Combine(basePath, $"{primaryAssembly.GetName().Name}.xml"), true);
                options.IncludeXmlComments(Path.Combine(basePath, $"{infrastructureAssembly.GetName().Name}.xml"), true);
                options.SchemaFilter<DisplayEnumDescFilter>();
            });
        }
    }
    public class UseSwagger : IAppEngine
    {
        private readonly WebApplication app;

        public UseSwagger(WebApplication app)
        {
            this.app = app;
        }
        public void Run()
        {
            if (!app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    typeof(SwaggerApiGroupNames).GetFields().Skip(1).ToList().ForEach(f =>
                    {
                        var info = f.GetCustomAttributes(typeof(SwaggerGroupInfoAttribute), false).OfType<SwaggerGroupInfoAttribute>().FirstOrDefault();
                        options.SwaggerEndpoint($"/swagger/{f.Name}/swagger.json", (info != null ? info.Title : f.Name) + "-" + app.Environment.EnvironmentName);
                    });
                    options.SwaggerEndpoint("/swagger/Other/swagger.json", "其他" + "-" + app.Environment.EnvironmentName);
                });
            }
        }
    }
}
