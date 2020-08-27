using System;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries
{
    public class IncentiveApplicationLegalEntityDto
    {
        public Guid ApplicationId { get; set; }
        public long LegalEntityId { get; set; }
        public IncentiveApplicationStatus ApplicationStatus { get; set; }
        public string VrfCaseId { get; set; }
    }
}
