using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class RecalculateEarningsRequest
    {
        public List<IncentiveLearnerIdentifierDto> IncentiveLearnerIdentifiers { get; set; }
    }
}
