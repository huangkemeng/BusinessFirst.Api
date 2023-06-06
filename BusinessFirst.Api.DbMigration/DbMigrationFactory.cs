using RenameMe.Api.Infrastructure.EfCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RenameMe.Api.DbMigration
{
    public class DbMigrationFactory : IDesignTimeDbContextFactory<SqlDbContext>
    {
        public SqlDbContext CreateDbContext(string[] args)
        {
            var basePath = AppContext.BaseDirectory;
            var dbSetting = new DbSetting();
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(Path.Combine(basePath, dbSetting.JsonFilePath), true);
            var configuration = configurationBuilder.Build();
            configuration.GetSection(nameof(DbSetting)).Bind(dbSetting);
            return new SqlDbContext(dbSetting);
        }
    }
}
