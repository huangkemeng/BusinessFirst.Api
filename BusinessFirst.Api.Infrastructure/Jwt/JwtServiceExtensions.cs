using Microsoft.Extensions.DependencyInjection;

namespace RenameMe.Api.Infrastructure.Jwt
{
    public static class JwtServiceExtensions
    {
        public static IServiceCollection AddJwtService(this IServiceCollection services)
        {
            return services.AddSingleton<JwtService>();
        }
    }
}
