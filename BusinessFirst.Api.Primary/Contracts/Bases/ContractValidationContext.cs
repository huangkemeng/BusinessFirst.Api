using FluentValidation;
using Mediator.Net.Contracts;

namespace RenameMe.Api.Primary.Contracts.Bases
{
    public class ContractValidationContext : ValidationContext<IMessage>
    {
        public ContractValidationContext(IMessage message) : base(message)
        {

        }
    }
}
