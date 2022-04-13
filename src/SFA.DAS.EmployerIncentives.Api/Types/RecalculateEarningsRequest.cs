using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Commands;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class RecalculateEarningsRequest
    {
        public List<IncentiveLearnerIdentifier> IncentiveLearnerIdentifiers { get; set; }
    }
}
