using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Activities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.Orchestrators
{
    public class EmailOrchestrator
    {
        [FunctionName(nameof(EmailOrchestrator))]
        public async Task RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var input = new SendMetricsReportEmailInput
            {
                CollectionPeriod = new CollectionPeriod { Period = 42, Year = 2442 },
                EmailAddress = "mihail.andrici@digital.education.gov.uk"
            };
            
            await context.CallActivityAsync(nameof(SendMetricsReportEmail), input);
        }
    }
}
