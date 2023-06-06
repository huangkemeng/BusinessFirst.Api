using RenameMe.Api.Infrastructure.EfCore;
using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ExceptionServices;

namespace RenameMe.Api.Engines.Mediator
{
    public class EfCorePipe : IPipeSpecification<IReceiveContext<IMessage>>
    {
        private readonly SqlDbContext dbContext;
        public EfCorePipe(IDependencyScope dependencyScope)
        {
            dbContext = dependencyScope.Resolve<SqlDbContext>();
        }
        public bool ShouldExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
        {
            return true;
        }
        public Task BeforeExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
        {
            return Task.WhenAll();
        }
        public Task Execute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
        {
            return Task.WhenAll();
        }
        public async Task AfterExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
        {
            if (dbContext != null)
            {
                //有DML操作才需要SaveChangesAsync
                var hasDML = dbContext.ChangeTracker.Entries().Any(e => new[] { EntityState.Deleted, EntityState.Modified, EntityState.Added }.Contains(e.State));
                if (hasDML)
                {
                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public Task OnException(System.Exception ex, IReceiveContext<IMessage> context)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();
            throw ex;
        }
    }
}
