using RenameMe.Api.Infrastructure.Bases;

namespace RenameMe.Api.Infrastructure.EfCore
{
    public class DbSetting : IJsonFileSetting
    {
        public string ConnectionString { get; set; }
        public string JsonFilePath => "./EfCore/db-setting.json";
    }
}
