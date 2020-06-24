using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock
{
    public interface IDistributedLockProvider
    {
        Task<bool> AcquireLock(string Id, CancellationToken cancellationToken);

        Task ReleaseLock(string Id);

        Task Start();

        Task Stop();
    }
}
