namespace RenameMe.Api.Primary.Entities.Bases
{
    /// <summary>
    /// 数据包含版本号请基于本接口
    /// </summary>
    public interface IMultipleVersion
    {
        public int VersionNumber { get; set; }

        public string? VersionName { get; set; }

    }
}
