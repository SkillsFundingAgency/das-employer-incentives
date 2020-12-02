﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{
    public static class WaitForFunction
    {
        [FunctionName(nameof(WaitForFunction))]
        [NoAutomaticTrigger]
        public static async Task Run([DurableClient] IDurableOrchestrationClient client, string name, TimeSpan? timeout)
        {
            using var cts = new CancellationTokenSource();
            if (timeout != null)
            {
                cts.CancelAfter(timeout.Value);
            }

            await client.Wait(status => status.All(x => x.Name != name), cts.Token);
        }
    }
}
