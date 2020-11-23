using System;

namespace SFA.DAS.EmployerIncentives.Functions.PaymentsProcess.AcceptanceTests.AzureFunctions
{
    public class OrchestrationStatus
    {
        public string Name { get; set; }
        public string InstanceId { get; set; }
        public string RuntimeStatus { get; set; }
        public object Input { get; set; }
        public string CustomStatus { get; set; }
        public string Output { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }
}
