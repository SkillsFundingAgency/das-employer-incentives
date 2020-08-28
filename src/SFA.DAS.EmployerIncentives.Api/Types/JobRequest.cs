using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public enum JobType
    {
        RefreshLegalEntities = 1,
        UpdateVrfCaseDetailsForNewApplications = 2
    }

    public class JobRequest
    {
        public JobType Type { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}
