using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{
    public class OrchestrationData : IOrchestrationData
    {  
        public DurableOrchestrationStatus Status { get; set; }
    }
}
