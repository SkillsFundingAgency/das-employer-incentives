using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models
{
    public class EmploymentCheckModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
        public Guid ApprenticeshipIncentiveId { get; set; }
        public EmploymentCheckType CheckType { get; set; }
        public DateTime MinimumDate { get; set; }
        public DateTime MaximumDate { get; set; }
        public Guid CorrelationId { get; set; }
        public bool? Result { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public DateTime? ResultDateTime { get; set; }
    }
}
