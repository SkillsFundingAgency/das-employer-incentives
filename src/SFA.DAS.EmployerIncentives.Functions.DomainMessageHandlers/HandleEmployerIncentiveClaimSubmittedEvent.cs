using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.EmployerIncentives.Messages.Events;
using SFA.DAS.NServiceBus.AzureFunction.Attributes;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public class HandleEmployerIncentiveClaimSubmittedEvent
    {
        [FunctionName("HandleEmployerIncentiveClaimSubmitted")]
        public Task RunEvent([NServiceBusTrigger(Endpoint = QueueNames.EmployerIncentiveClaimSubmitted)] EmployerIncentiveClaimSubmittedEvent message)
        {
            return Task.CompletedTask;
        }
    }
}
