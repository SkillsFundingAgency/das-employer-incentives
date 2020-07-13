using System;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using SFA.DAS.EmployerIncentives.Infrastructure.Configuration;
using SFA.DAS.EmployerIncentives.Queries.Exceptions;

namespace SFA.DAS.EmployerIncentives.Queries
{
    public class Policies
    {
        public readonly AsyncRetryPolicy LockRetryPolicy;

        public Policies(IOptions<RetryPolicies> settings)
        {
            var retryPolicies = settings.Value;

            LockRetryPolicy = Policy
                .Handle<QueryException>()
                .WaitAndRetryAsync(
                    retryPolicies.LockedRetryAttempts,
                    retryAttempt => TimeSpan.FromMilliseconds(retryPolicies.LockedRetryWaitInMilliSeconds));
        }
    }
}
