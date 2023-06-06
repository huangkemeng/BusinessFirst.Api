using RenameMe.Api.Infrastructure.Jwt;
using RenameMe.Api.Realization.Bases;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace RenameMe.Api.Engines.Jwt
{
    public class ConfigureJwt //: IBuilderEngine
    {
        private readonly IServiceCollection services;

        public ConfigureJwt(IServiceCollection services)
        {
            this.services = services;
        }
        public void Run()
        {
            services
                .AddJwtService()
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer((options) =>
            {
                var provider = services.BuildServiceProvider();
                var jwtSetting = provider.GetService<JwtSetting>() ?? throw new BusinessException($"获取配置文件{nameof(JwtSetting)}失败", BusinessExceptionTypeEnum.Configuration);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSetting!.Issuer,
                    ValidAudience = jwtSetting.Audience,
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSetting.SignKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
                options.Events = new JwtBearerEvents()
                {
                    OnTokenValidated = context =>
                    {
                        var principal = context.Principal;
                        if (principal != null)
                        {
                            var tokenType = principal.FindFirstValue(JwtService.TokenTypeConst);
                            if (tokenType == TokenTypeEnum.AccessToken.ToString())
                            {
                                return Task.CompletedTask;
                            }
                        }
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                };
            });
        }
    }

    public class UseJwt //: IAppEngine
    {
        private readonly WebApplication app;

        public UseJwt(WebApplication app)
        {
            this.app = app;
        }
        public void Run()
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
