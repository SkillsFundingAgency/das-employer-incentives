using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Commands;

namespace SFA.DAS.EmployerIncentives.Api.Types
{
    public class UpdateIncentiveApplicationRequest
    {
        public Guid IncentiveApplicationId { get; set; }
        public long AccountId { get; set; }
        public IEnumerable<IncentiveApplicationApprenticeshipDto> Apprenticeships { get; set; }
    }
}
