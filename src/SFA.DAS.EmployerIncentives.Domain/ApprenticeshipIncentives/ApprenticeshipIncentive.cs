using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public sealed class ApprenticeshipIncentive : AggregateRoot<Guid, ApprenticeshipIncentiveModel>
    {
        public Account Account => Model.Account;
        public Apprenticeship Apprenticeship => Model.Apprenticeship;
        public IReadOnlyCollection<PendingPayment> PendingPayments => Model.PendingPaymentModels.Map().ToList().AsReadOnly();

        internal static ApprenticeshipIncentive New(Guid id, Account account, Apprenticeship apprenticeship)
        {
            return new ApprenticeshipIncentive(id, new ApprenticeshipIncentiveModel { Id = id, Account = account, Apprenticeship = apprenticeship }, true);
        }

        internal static ApprenticeshipIncentive Get(Guid id, ApprenticeshipIncentiveModel model)
        {
            return new ApprenticeshipIncentive(id, model);
        }
        public void AddIncentive(Incentive incentive)
        {
            if(!incentive.IsEligible)
            {
                throw new InvalidIncentiveException("Incentive does not pass the eligibility checks");
            }

            foreach (var payment in incentive.Payments)
            {
                Model.PendingPaymentModels.Add(
                       PendingPayment.New(
                           Guid.NewGuid(),
                           Model.Account,
                           Model.Id,
                           payment.Amount,
                           payment.PaymentDate,
                           DateTime.Now)
                       .GetModel());
            }
        }

        private ApprenticeshipIncentive(Guid id, ApprenticeshipIncentiveModel model, bool isNew = false) : base(id, model, isNew)
        {            
        }
    }
}
