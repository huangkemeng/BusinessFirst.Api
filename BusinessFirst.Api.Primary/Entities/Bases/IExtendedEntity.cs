namespace RenameMe.Api.Primary.Entities.Bases
{
    /// <summary>
    /// 扩展表请基于本接口
    /// </summary>
    public interface IExtendedEntity<T> : IEntityPrimary where T : IEntityPrimary
    {
        Guid MainId { get; set; }
    }
}
