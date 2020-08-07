using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class IncentiveApplicationDto
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }

        public IEnumerable<IncentiveApplicationApprenticeshipDto> Apprenticeships { get; set; }
    }
}
