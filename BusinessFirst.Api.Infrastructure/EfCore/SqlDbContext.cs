using RenameMe.Api.Infrastructure.EfCore.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace RenameMe.Api.Infrastructure.EfCore
{
    public class SqlDbContext : DbContext
    {
        private readonly DbSetting dbSetting;

        public SqlDbContext(DbSetting dbSetting) : base()
        {
            this.dbSetting = dbSetting;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // 全局限制查询的条数 (Top1000)
            //optionsBuilder.ReplaceService<IQueryTranslationPostprocessorFactory, TopRowsQueryTranslationPostprocessorFactory>();
            optionsBuilder.UseSqlServer(dbSetting.ConnectionString, options =>
            {
                options.CommandTimeout(6000);
            });
            if (Debugger.IsAttached)
            {
                optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.LoadFromEntityConfigure();
        }
    }
}
