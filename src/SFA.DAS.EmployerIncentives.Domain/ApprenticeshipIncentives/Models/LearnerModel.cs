using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using System;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models
{
    public class LearnerModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public long ApprenticeshipId { get; set; }
        public long Ukprn { get; set; }
        public long UniqueLearnerNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public SubmissionData SubmissionData { get; set; }
    }
}
