using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
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
        public DateTime PlannedStartDate => Model.PlannedStartDate;        
        public IReadOnlyCollection<PendingPayment> PendingPayments => Model.PendingPaymentModels.Map().ToList().AsReadOnly();
        
        internal static ApprenticeshipIncentive New(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime plannedStartDate)
        {
            return new ApprenticeshipIncentive(
                id, 
                new ApprenticeshipIncentiveModel { 
                    Id = id, 
                    ApplicationApprenticeshipId = applicationApprenticeshipId,
                    Account = account, 
                    Apprenticeship = apprenticeship, 
                    PlannedStartDate = plannedStartDate }, true);
        }

        internal static ApprenticeshipIncentive Get(Guid id, ApprenticeshipIncentiveModel model)
        {
            return new ApprenticeshipIncentive(id, model);
        }

        public void CalculateEarnings(IEnumerable<IncentivePaymentProfile> paymentProfiles, CollectionCalendar collectionCalendar)
        {
            var incentive = new Incentive(Apprenticeship.DateOfBirth, PlannedStartDate, paymentProfiles);
            if(!incentive.IsEligible)
            {
                throw new InvalidIncentiveException("Incentive does not pass the eligibility checks");
            }

            Model.PendingPaymentModels.Clear();
            foreach (var payment in incentive.Payments)
            {
                 var pendingPayment = PendingPayment.New(
                               Guid.NewGuid(),
                               Model.Account,
                               Model.Id,
                               payment.Amount,
                               payment.PaymentDate,
                               DateTime.Now,
                               payment.PaymentNumber);

                pendingPayment.SetPaymentPeriod(collectionCalendar);
                
                Model.PendingPaymentModels.Add(pendingPayment.GetModel());
            }

            AddEvent(new EarningsCalculated
            {
                ApprenticeshipIncentiveId = Id,
                AccountId = Account.Id,
                ApprenticeshipId = Apprenticeship.Id,
                ApplicationApprenticeshipId = Model.ApplicationApprenticeshipId
            });
        }

        private ApprenticeshipIncentive(Guid id, ApprenticeshipIncentiveModel model, bool isNew = false) : base(id, model, isNew)
        {            
            if(isNew)
            {
                AddEvent(new Created
                {
                    ApprenticeshipIncentiveId = id,
                    AccountId = model.Account.Id,
                    ApprenticeshipId =  model.Apprenticeship.Id
                });
            }
        }
    }
}
