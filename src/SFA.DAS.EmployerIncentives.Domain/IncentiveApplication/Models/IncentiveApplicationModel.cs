using System;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplication.Models
{
    public class IncentiveApplicationModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public DateTime DateCreated { get; set; }
        public IncentiveApplicationStatus Status { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string SubmittedBy { get; set; }
    }
}
