
namespace RenameMe.Api.Primary.Entities.Bases
{
    public static class EntityDefaultValueSet
    {
        public static void InitProperties<T>(this T entity) where T : IEntityPrimary
        {
            entity.Id = Guid.NewGuid();
            if (entity is IMainEntity main)
            {
                main.CreatedOn = DateTimeOffset.Now;
            }
            if (entity is IMultipleVersion version)
            {
                version.VersionNumber = 1;
            }
        }
    }
}
