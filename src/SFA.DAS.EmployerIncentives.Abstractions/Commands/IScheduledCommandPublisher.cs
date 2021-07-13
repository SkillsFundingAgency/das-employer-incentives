using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Abstractions.Commands
{
    public interface IScheduledCommandPublisher
    {
        Task Send<T>(T command, TimeSpan delay, CancellationToken cancellationToken = default(CancellationToken)) where T : ICommand;
    }
}
