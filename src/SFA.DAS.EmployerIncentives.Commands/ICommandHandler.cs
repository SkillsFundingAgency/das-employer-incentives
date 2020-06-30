using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public interface ICommandHandler<in TCommand> where TCommand: ICommand
    {
        Task HandleAsync(TCommand command);
    }
}
