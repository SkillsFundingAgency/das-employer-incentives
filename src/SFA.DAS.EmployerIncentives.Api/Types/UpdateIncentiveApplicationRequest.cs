using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Commands;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class UpdateIncentiveApplicationRequest
    {
        public Guid IncentiveApplicationId { get; set; }
        public long AccountId { get; set; }
        public IEnumerable<IncentiveApplicationApprenticeshipDto> Apprenticeships { get; set; }
    }
}
