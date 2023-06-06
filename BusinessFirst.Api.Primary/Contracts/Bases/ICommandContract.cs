using Mediator.Net.Contracts;

namespace RenameMe.Api.Primary.Contracts.Bases
{
    public interface ICommandContract<TCommand, TResponse> : IContract<TCommand>, ICommandHandler<TCommand, TResponse> where TCommand : ICommand where TResponse : IResponse
    {
    }

    public interface ICommandContract<TCommand> : IContract<TCommand>, ICommandHandler<TCommand> where TCommand : ICommand
    {
    }
}
