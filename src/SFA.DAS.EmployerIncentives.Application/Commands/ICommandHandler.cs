using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Application.Commands
{
    public interface ICommandHandler<in T> where T: ICommand
    {
        Task Handle(T command, CancellationToken cancellationToken = default(CancellationToken));
    }
}
