using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands.AddLegalEntity
{
    public class AddLegalEntityCommandHandler : ICommandHandler<AddLegalEntityCommand>
    {
        public Task Handle(AddLegalEntityCommand command)
        {
            return Task.CompletedTask;
        }
    }
}
