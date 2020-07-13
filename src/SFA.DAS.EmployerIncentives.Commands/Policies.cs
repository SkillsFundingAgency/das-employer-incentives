using Microsoft.Extensions.Options;
using Polly;
using Polly.Bulkhead;
using Polly.Retry;
using SFA.DAS.EmployerIncentives.Commands.Exceptions;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using System;

namespace SFA.DAS.EmployerIncentives.Commands
{
    public class Policies
    {
        public readonly AsyncRetryPolicy LockRetryPolicy;
        public readonly AsyncBulkheadPolicy MultiEventPublishLimiterPolicy;

        public Policies(IOptions<PolicySettings> policySettings)
        {
            var retryPolicies = policySettings.Value?.RetryPolicies;

            LockRetryPolicy = Policy
                .Handle<EntityLockedException>()
                .WaitAndRetryAsync(
                    retryPolicies?.LockedRetryAttempts ?? 3,
                    retryAttempt => TimeSpan.FromMilliseconds(retryPolicies?.LockedRetryWaitInMilliSeconds ?? 5000));

            var maxParallelization = policySettings.Value?.MultiEventPublisherLimitPolicy?.MaxParallelization ?? 100;
            MultiEventPublishLimiterPolicy = Policy.BulkheadAsync(maxParallelization, int.MaxValue);
        }
    }
}
