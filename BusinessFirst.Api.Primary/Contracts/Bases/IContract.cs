using Mediator.Net.Contracts;

namespace RenameMe.Api.Primary.Contracts.Bases
{
    public interface IContract<T> where T : IMessage
    {
        Task TestAsync();
        void Validator(ContractValidator<T> validator);
    }
}
