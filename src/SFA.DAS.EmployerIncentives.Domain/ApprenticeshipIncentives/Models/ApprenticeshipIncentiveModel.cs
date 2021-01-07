using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models
{
    public class ApprenticeshipIncentiveModel : IEntityModel<Guid>
    {
        public Guid Id { get; set; }
        public Account Account { get; set; }        
        public Apprenticeship Apprenticeship { get; set; }
        public DateTime StartDate { get; set; }
        public Guid ApplicationApprenticeshipId { get; set; }
        public bool RefreshedLearnerForEarnings { get; set; }
        public bool HasPossibleChangeOfCircumstances { get; set; }
        public ICollection<PendingPaymentModel> PendingPaymentModels { get; set; }
        public ICollection<PaymentModel> PaymentModels { get; set; }
        public bool PausePayments { get; set; }

        public ApprenticeshipIncentiveModel()
        {
            PendingPaymentModels = new List<PendingPaymentModel>();
            PaymentModels = new List<PaymentModel>();
        }
    }
}
