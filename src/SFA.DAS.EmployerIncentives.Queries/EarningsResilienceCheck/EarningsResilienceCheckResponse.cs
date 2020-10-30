using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmployerIncentives.Queries.EarningsResilienceCheck
{
    public class EarningsResilienceCheckResponse
    {
        public IEnumerable<EarningsResilienceCheckDto> CheckResults { get; set; }
    }
}
