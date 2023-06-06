using RenameMe.Api.Primary.Contracts.Bases;
using RenameMe.Api.Realization.Bases;
using FluentValidation;
using Mediator.Net;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using System.Runtime.ExceptionServices;

namespace RenameMe.Api.Engines.Mediator
{
    public class DoValidatePipe : IPipeSpecification<IReceiveContext<IMessage>>
    {
        private readonly IDependencyScope dependencyScope;

        public DoValidatePipe(IDependencyScope dependencyScope)
        {
            this.dependencyScope = dependencyScope;
        }
        public bool ShouldExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
        {
            return true;
        }

        public Task BeforeExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
        {
            return Task.WhenAll();
        }

        public async Task Execute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
        {
            if (ShouldExecute(context, cancellationToken))
            {
                if (dependencyScope != null && context.Message != null)
                {
                    var msgType = context.Message.GetType();
                    var ivalidatorType = typeof(IValidator<>).MakeGenericType(msgType);
                    if (dependencyScope.Resolve(ivalidatorType) is IValidator validator && validator.CanValidateInstancesOfType(msgType))
                    {
                        var result = await validator.ValidateAsync(new ContractValidationContext(context.Message), cancellationToken);
                        if (!result.IsValid)
                        {
                            var validationMessages = result.Errors.Select(e => e.ErrorMessage).ToList();
                            throw new BusinessException(validationMessages, BusinessExceptionTypeEnum.Validator);
                        }
                    }
                }
            }
        }

        public Task AfterExecute(IReceiveContext<IMessage> context, CancellationToken cancellationToken)
        {
            return Task.WhenAll();
        }

        public Task OnException(System.Exception ex, IReceiveContext<IMessage> context)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();
            throw ex;
        }
    }
}
