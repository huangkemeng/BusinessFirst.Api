namespace RenameMe.Api.Primary.Entities.Bases
{
    /// <summary>
    /// 所有的表都应该基于本接口  
    /// </summary>
    public interface IEntityPrimary
    {
        Guid Id { get; set; }
    }
}
