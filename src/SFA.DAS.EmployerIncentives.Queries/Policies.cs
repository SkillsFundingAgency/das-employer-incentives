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
        public readonly AsyncRetryPolicy QueryRetryPolicy;

        public Policies(IOptions<PolicySettings> policySettings)
        {
            var retryPolicies = policySettings.Value?.RetryPolicies;

            QueryRetryPolicy = Policy
                .Handle<QueryException>()
                .WaitAndRetryAsync(
                    retryPolicies.QueryRetryAttempts,
                    retryAttempt => TimeSpan.FromMilliseconds(retryPolicies.QueryRetryWaitInMilliSeconds));
        }
    }
}
