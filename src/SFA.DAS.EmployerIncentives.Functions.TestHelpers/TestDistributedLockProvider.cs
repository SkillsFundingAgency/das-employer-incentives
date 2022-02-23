using SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{
    [ExcludeFromCodeCoverage]
    public class TestDistributedLockProvider : IDistributedLockProvider
    {
        private static readonly HashSet<string> _locks = new HashSet<string>();

        public Task<bool> AcquireLock(string Id, CancellationToken cancellationToken)
        {
            lock (_locks)
            {
                if (_locks.Contains(Id))
                {
                    return Task.FromResult(false);
                }
                else
                {
                    _locks.Add(Id);
                    return Task.FromResult(true);
                }
            }
        }

        public Task ReleaseLock(string Id)
        {
            lock (_locks)
            {
                _locks.Remove(Id);
            }

            return Task.CompletedTask;
        }

        public Task Start()
        {
            return Task.CompletedTask;
        }

        public Task Stop()
        {
            return Task.CompletedTask;
        }
    }
}