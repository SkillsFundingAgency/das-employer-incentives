using System;

namespace SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives
{
    public class ApprenticeshipIncentiveDto
    {
        public Guid Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public long ULN { get; set; }
        public long? UKPRN { get; set; }
    }
}
