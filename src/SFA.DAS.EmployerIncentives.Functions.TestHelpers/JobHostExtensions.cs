using Microsoft.Azure.WebJobs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{

    public static class JobHostExtensions
    {
        public static async Task<IJobHost> Ready(this IJobHost jobs, TimeSpan? timeout = null)
        {
            await jobs.CallAsync(nameof(ReadyFunction), new Dictionary<string, object> { ["timeout"] = timeout });
            return jobs;
        }

        public static async Task RefreshStatus(this IJobHost jobs, string instanceId)
        {
            await jobs.CallAsync(nameof(GetStatusFunction), new Dictionary<string, object>
            {
                ["instanceId"] = instanceId
            });
        }

        public static async Task<IJobHost> Ready(this Task<IJobHost> task, TimeSpan? timeout = null)
        {
            var jobs = await task;
            return await jobs.Ready(timeout);
        }

        public static async Task<IJobHost> Start(this IJobHost jobs, OrchestrationStarterInfo starterInfo)
        {
            await jobs.CallAsync(starterInfo.StarterName, starterInfo.StarterArgs);

            await jobs.WaitFor(starterInfo.OrchestrationName, starterInfo.Timeout).ThrowIfFailed();

            return jobs;
        }
        public static async Task<IJobHost> WaitFor(this IJobHost jobs, string orchestration, TimeSpan? timeout = null)
        {
            await jobs.CallAsync(nameof(WaitForFunction), new Dictionary<string, object>
            {
                ["timeout"] = timeout,
                ["name"] = orchestration
            });

            return jobs;
        }
        
        public static async Task<IJobHost> WaitFor(this Task<IJobHost> task, string orchestration, TimeSpan? timeout = null)
        {
            var jobs = await task;
            return await jobs.WaitFor(orchestration, timeout);
        }
                
        public static async Task<IJobHost> ThrowIfFailed(this Task<IJobHost> task)
        {
            var jobs = await task;
            await jobs.CallAsync(nameof(ThrowIfFailedFunction));

            return jobs;
        }

        public static async Task<IJobHost> Purge(this Task<IJobHost> task)
        {
            var jobs = await task;
            await jobs.CallAsync(nameof(PurgeFunction));

            return jobs;
        }

        public static async Task<IJobHost> Purge(this IJobHost jobs)
        {
            await jobs.CallAsync(nameof(PurgeFunction));
            return jobs;
        }
    
        public static async Task<IJobHost> Terminate(this IJobHost jobs)
        {
            await jobs.CallAsync(nameof(TerminateFunction));
            return jobs;
        }
    }
}
