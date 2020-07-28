using System;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplication.Models
{
    public class IncentiveApplicationModel : IEntityModel<Guid>
    {
        public IncentiveApplicationModel()
        {
            ApprenticeshipModels = new List<ApprenticeshipModel>();
        }

        public Guid Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public DateTime DateCreated { get; set; }
        public IncentiveApplicationStatus Status { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string SubmittedBy { get; set; }
        public ICollection<ApprenticeshipModel> ApprenticeshipModels { get; set; }
    }
}
