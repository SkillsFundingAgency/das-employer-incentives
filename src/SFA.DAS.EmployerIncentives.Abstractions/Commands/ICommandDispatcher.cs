using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Commands
{
    public interface ICommandDispatcher
    {
        Task Send<TCommand>(TCommand command, CancellationToken cancellationToken = default(CancellationToken)) where TCommand : ICommand;
        Task SendMany<TCommands>(TCommands commands, CancellationToken cancellationToken = default(CancellationToken)) where TCommands : IEnumerable<ICommand>;
    }
}
