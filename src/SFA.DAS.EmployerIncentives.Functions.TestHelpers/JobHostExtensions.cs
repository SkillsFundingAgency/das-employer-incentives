using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{

    public static class JobHostExtensions
    {
        public static async Task<IJobHost> Ready(this IJobHost jobs, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            await jobs.CallAsync(nameof(ReadyFunction), new Dictionary<string, object> { ["timeout"] = timeout }, cancellationToken);
            return jobs;
        }

        public static async Task RefreshStatus(this IJobHost jobs, string instanceId, CancellationToken cancellationToken = default)
        {
            await jobs.CallAsync(nameof(GetStatusFunction), new Dictionary<string, object>
            {
                ["instanceId"] = instanceId
            }, cancellationToken);
        }

        public static async Task<IJobHost> Ready(this Task<IJobHost> task, TimeSpan? timeout = null)
        {
            var jobs = await task;
            return await jobs.Ready(timeout);
        }

        public static async Task<IJobHost> Start(this IJobHost jobs, EndpointInfo endpointInfo, CancellationToken cancellationToken = default)
        {
            await jobs.CallAsync(endpointInfo.StarterName, endpointInfo.StarterArgs, cancellationToken);
            return jobs;
        }

        public static async Task<IJobHost> Start(this IJobHost jobs, 
            OrchestrationStarterInfo starterInfo,
            bool throwIfFailed,
            CancellationToken cancellationToken = default)
        {
            await jobs.CallAsync(starterInfo.StarterName, starterInfo.StarterArgs, cancellationToken);

            if (throwIfFailed)
            {
                await jobs.WaitFor(starterInfo.OrchestrationName, starterInfo.Timeout, starterInfo.ExpectedCustomStatus).ThrowIfFailed();
            }
            else
            {
                await jobs.WaitFor(starterInfo.OrchestrationName, starterInfo.Timeout, starterInfo.ExpectedCustomStatus);
            }

            return jobs;
        }
        public static async Task<IJobHost> WaitFor(this IJobHost jobs, string orchestration, TimeSpan? timeout = null, string expectedCustomStatus = null, CancellationToken cancellationToken = default)
        {
            await jobs.CallAsync(nameof(WaitForFunction), new Dictionary<string, object>
            {
                ["timeout"] = timeout,
                ["name"] = orchestration,
                ["expectedCustomStatus"] = expectedCustomStatus
            }, cancellationToken);

            return jobs;
        }

        public static async Task<IJobHost> WaitFor(this Task<IJobHost> task, string orchestration, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            var jobs = await task;
            return await jobs.WaitFor(orchestration, timeout, cancellationToken: cancellationToken);
        }
                
        public static async Task<IJobHost> ThrowIfFailed(this Task<IJobHost> task, CancellationToken cancellationToken = default)
        {
            var jobs = await task;
            await jobs.CallAsync(nameof(ThrowIfFailedFunction), cancellationToken: cancellationToken);

            return jobs;
        }

        public static async Task<IJobHost> Purge(this Task<IJobHost> task, CancellationToken cancellationToken = default)
        {
            var jobs = await task;
            await jobs.CallAsync(nameof(PurgeFunction), cancellationToken: cancellationToken);

            return jobs;
        }

        public static async Task<IJobHost> Purge(this IJobHost jobs, CancellationToken cancellationToken = default)
        {
            await jobs.CallAsync(nameof(PurgeFunction), cancellationToken: cancellationToken);
            return jobs;
        }
    
        public static async Task<IJobHost> Terminate(this IJobHost jobs, CancellationToken cancellationToken = default)
        {
            await jobs.CallAsync(nameof(TerminateFunction), cancellationToken: cancellationToken);
            return jobs;
        }
    }
}
