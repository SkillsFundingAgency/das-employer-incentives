using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.HouseKeeping
{
    public class CleanupWorkflows_Timer
    {
        [FunctionName(nameof(CleanupOldWorkflows))]
        public async Task CleanupOldWorkflows([TimerTrigger("0 0 0 * * *")] TimerInfo timerInfo,
            [DurableClient] IDurableOrchestrationClient orchestrationClient, ILogger log)
        {
            var createdTimeFrom = DateTime.UtcNow.Subtract(TimeSpan.FromDays(365));
            var createdTimeTo = DateTime.UtcNow.Subtract(TimeSpan.FromDays(3));
            var runtimeStatus = new List<OrchestrationStatus>
            {
                OrchestrationStatus.Completed,
                OrchestrationStatus.Canceled,
                OrchestrationStatus.ContinuedAsNew,
                OrchestrationStatus.Failed,
                OrchestrationStatus.Terminated,
            };
            var result = await orchestrationClient.PurgeInstanceHistoryAsync(createdTimeFrom, createdTimeTo, runtimeStatus);
            log.LogInformation("Scheduled cleanup done, {InstancesDeleted} instances deleted", result.InstancesDeleted);
        }
    }
}
