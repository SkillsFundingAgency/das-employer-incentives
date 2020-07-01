﻿using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Infrastructure.DistributedLock
{
    public class NullLockProvider : IDistributedLockProvider
    {
        public Task<bool> AcquireLock(string Id, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task ReleaseLock(string Id)
        {
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
