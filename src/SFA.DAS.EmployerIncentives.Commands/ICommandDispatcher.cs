using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public interface ICommandDispatcher
    {
        Task<TResponse> Send<TResponse, TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : ICommand;
        Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : ICommand;
    }
}
