using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Commands
{
    public interface ICommandPublisher
    {
        Task Publish<T>(T command, CancellationToken cancellationToken = default(CancellationToken)) where T : ICommand;
        Task Publish(object command, CancellationToken cancellationToken = default(CancellationToken));
    }
}
