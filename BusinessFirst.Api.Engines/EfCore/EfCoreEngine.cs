using Autofac;
using RenameMe.Api.Engines.Bases;
using RenameMe.Api.Infrastructure.EfCore;
using RenameMe.Api.Primary.Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace RenameMe.Api.Engines.EfCore
{
    public class ConfigureEfCore : IBuilderEngine
    {
        private readonly ContainerBuilder container;

        public ConfigureEfCore(ContainerBuilder container)
        {
            this.container = container;
        }
        public void Run()
        {
            container.RegisterType<SqlDbContext>()
                   .AsSelf()
                   .As<DbContext>()
                   .InstancePerLifetimeScope();
            var idbEntityType = typeof(IEfDbEntity<>);
            var idbEntityAssembly = idbEntityType.Assembly;
            var dbEntityTypes = idbEntityAssembly
                ?.ExportedTypes
                .Where(e => e.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == idbEntityType) && e.IsClass && !e.IsAbstract)
                .ToArray();
            if (dbEntityTypes != null && dbEntityTypes.Any())
            {
                var dbsetType = typeof(SqlDbContext).GetMethods().First(e => e.Name == nameof(DbContext.Set) && e.GetParameters().Length == 0);
                foreach (var dbEntityType in dbEntityTypes)
                {
                    var dbsetGenericType = dbsetType.MakeGenericMethod(dbEntityType);
                    container.Register(c =>
                    {
                        var dbContext = c.Resolve<SqlDbContext>();
                        return dbsetGenericType.Invoke(dbContext, null);
                    })
                     .As(typeof(DbSet<>).MakeGenericType(dbEntityType));
                }
            }
        }
    }

    public class UseEfCore : IAppEngine
    {
        private readonly ILifetimeScope migrateScope;

        public UseEfCore(ILifetimeScope migrateScope)
        {
            this.migrateScope = migrateScope;
        }
        public void Run()
        {
            using var scope = migrateScope.BeginLifetimeScope();
            var dbContext = scope.Resolve<DbContext>()!;
            var connectString = dbContext.Database.GetConnectionString();
            if (!string.IsNullOrWhiteSpace(connectString))
            {
                var migrations = dbContext.Database.GetMigrations();
                if (migrations.Any())
                {
                    if (dbContext.Database.CanConnect())
                    {
                        dbContext.Database.Migrate();
                    }
                }
            }
        }
    }
}
