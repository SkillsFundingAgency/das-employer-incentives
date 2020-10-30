using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck
{
    public class EarningsResilienceCheckResponse
    {
        public IEnumerable<EarningsResilienceCheckDto> Applications { get; set; }
    }
}
