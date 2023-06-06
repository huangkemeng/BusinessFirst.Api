using RenameMe.Api.Primary.Contracts.Bases;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace RenameMe.Api.Infrastructure.EfCore.Bases
{
    public static class EfCoreQueryableExtensions
    {
        public static IQueryable<T> Sort<T>(this IQueryable<T> query, ISortable sortable) where T : class
        {
            if (sortable != null && sortable.Sort != null)
            {
                var type = typeof(T);
                var prop = type.GetProperties().FirstOrDefault(e => e.Name.Equals(sortable.Sort.FieldName, StringComparison.OrdinalIgnoreCase));
                if (prop != null)
                {
                    ParameterExpression parameterExpression = Expression.Parameter(type, "x");
                    MemberExpression memberExpression = Expression.MakeMemberAccess(parameterExpression, prop);
                    var filterExpress = Expression.Lambda<Func<T, object>>(Expression.Convert(memberExpression, typeof(object)), parameterExpression);
                    if (filterExpress != null)
                    {
                        if (sortable.Sort.Direction == SortDirectionEnum.Descending)
                        {
                            query = query.OrderByDescending(filterExpress);
                        }
                        else
                        {
                            query = query.OrderBy(filterExpress);
                        }
                    }
                }
            }
            return query;
        }
        public static async Task<PaginatedResult<T>> PaginateAsync<T>(this IQueryable<T> query, IPaginable paginable, CancellationToken cancellationToken) where T : class
        {
            PaginatedResult<T> paginatedResult = new()
            {
                Total = await query.CountAsync(cancellationToken)
            };
            if (paginable != null)
            {
                query = query.Skip((paginable.Offset - 1) * paginable.PageSize)
                             .Take(paginable.PageSize);
            }
            paginatedResult.List = await query.ToListAsync(cancellationToken);
            return paginatedResult;
        }
        public static IQueryable<T> WhereWhile<T>(this IQueryable<T> query, bool predicate, Expression<Func<T, bool>> expression) where T : class
        {
            if (predicate)
            {
                return query.Where(expression);
            }
            return query;
        }
        public static async Task Merge<T>(this DbSet<T> db, IQueryable<T> queryable, IEnumerable<T> mergingObjects, Func<T, T, bool> filter, Action<T, T> updateAction, CancellationToken cancellationToken) where T : class
        {
            var existingEntities = await queryable.ToListAsync(cancellationToken);
            var addingEntities = new List<T>();
            var updatingEntities = new List<T>();
            foreach (var mergingObject in mergingObjects)
            {
                var existingEntity = existingEntities.FirstOrDefault(e => filter(e, mergingObject));
                if (existingEntity != null)
                {
                    updateAction(existingEntity, mergingObject);
                    updatingEntities.Add(existingEntity);
                }
                else
                {
                    addingEntities.Add(mergingObject);
                }
            }
            db.UpdateRange(updatingEntities);
            var deletingEntites = existingEntities.Where(e => !updatingEntities.Any(x => filter(e, x))).ToList();
            db.RemoveRange(deletingEntites);
            await db.AddRangeAsync(addingEntities, cancellationToken);
        }
    }

    public class PaginatedResult<T>
    {
        public List<T> List { get; set; }

        public int Total { get; set; }
    }
}
