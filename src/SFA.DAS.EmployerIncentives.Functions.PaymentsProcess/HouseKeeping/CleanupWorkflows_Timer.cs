using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.HouseKeeping
{
    public class CleanupWorkflows_Timer
    {
        [Function(nameof(CleanupOldWorkflows))]
        public async Task CleanupOldWorkflows([TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo,
            [DurableClient] DurableTaskClient orchestrationClient, ILogger log)
        {
            var createdTimeFrom = DateTime.UtcNow.Subtract(TimeSpan.FromDays(365));
            var createdTimeTo = DateTime.UtcNow.Subtract(TimeSpan.FromDays(3));
            var runtimeStatus = new List<OrchestrationRuntimeStatus>
            {
                OrchestrationRuntimeStatus.Completed,
                OrchestrationRuntimeStatus.Failed,
                OrchestrationRuntimeStatus.Terminated
            };
            var result = await orchestrationClient.PurgeAllInstancesAsync(
                new PurgeInstancesFilter(createdTimeFrom, createdTimeTo, runtimeStatus)
                );
            log.LogInformation("Scheduled cleanup done, {InstancesDeleted} instances deleted", result.PurgedInstanceCount);

        }
    }
}
