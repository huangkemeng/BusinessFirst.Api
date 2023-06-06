namespace RenameMe.Api.Primary.Entities.Bases
{
    /// <summary>
    /// 主表请基于本接口
    /// </summary>
    public interface IMainEntity : IEntityPrimary
    {
        DateTimeOffset CreatedOn { get; set; }

        Guid CreatedBy { get; set; }

        DateTimeOffset? UpdatedOn { get; set; }

        Guid? UpdatedBy { get; set; }
    }
}
