using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.EmployerIncentives.Functions.EarningsResilienceCheck.Functions
{
    public static class EarningsResilienceCheck
    {
        [FunctionName("EarningsResilienceCheck")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
