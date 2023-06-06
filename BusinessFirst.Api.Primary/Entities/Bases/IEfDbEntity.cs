using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RenameMe.Api.Primary.Entities.Bases
{
    /// <summary>
    ///  如果一个实体后续会使用EfCore进行操作，请实现该接口
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEfDbEntity<T> where T : class, IEntityPrimary
    {
        public static abstract void EntityConfigure(EntityTypeBuilder<T> builder);
    }
}
