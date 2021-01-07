using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace SFA.DAS.EmployerIncentives.Functions.TestHelpers
{
    public interface IOrchestrationData
    {
        DurableOrchestrationStatus Status { get; set; }
    }
}
