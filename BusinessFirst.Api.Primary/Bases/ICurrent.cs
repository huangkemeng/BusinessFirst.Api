namespace RenameMe.Api.Primary.Bases
{
    public interface ICurrent<TEntity>
    {
        Task<TEntity?> FirstOrDefaultAsync(CancellationToken cancellationToken = default);
        Task<Guid?> GetCurrentUserIdAsync();
    }
}
