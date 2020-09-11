using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class IncentiveApplicationDto
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public long LegalEntityId { get; set; }
        public string SubmittedByEmail { get; set; }
        public string SubmittedByName { get; set; }
        public string VrfCaseStatus { get; set; }

        public IEnumerable<IncentiveApplicationApprenticeshipDto> Apprenticeships { get; set; }
    }
}
