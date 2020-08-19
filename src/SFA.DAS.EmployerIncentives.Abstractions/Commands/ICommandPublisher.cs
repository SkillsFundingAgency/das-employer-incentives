using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Commands
{
    public interface ICommandPublisher<TCommand>
    {
        Task Publish(TCommand command, CancellationToken cancellationToken = default(CancellationToken));
    }
}
