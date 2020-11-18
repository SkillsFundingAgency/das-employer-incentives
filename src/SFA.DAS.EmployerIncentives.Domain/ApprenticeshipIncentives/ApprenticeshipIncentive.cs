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
        public IReadOnlyCollection<Payment> Payments => Model.PaymentModels.Map().ToList().AsReadOnly();

        internal static ApprenticeshipIncentive New(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime plannedStartDate)
        {
            return new ApprenticeshipIncentive(
                id,
                new ApprenticeshipIncentiveModel
                {
                    Id = id,
                    ApplicationApprenticeshipId = applicationApprenticeshipId,
                    Account = account,
                    Apprenticeship = apprenticeship,
                    PlannedStartDate = plannedStartDate
                }, true);
        }

        internal static ApprenticeshipIncentive Get(Guid id, ApprenticeshipIncentiveModel model)
        {
            return new ApprenticeshipIncentive(id, model);
        }

        public void CalculateEarnings(IEnumerable<IncentivePaymentProfile> paymentProfiles, CollectionCalendar collectionCalendar)
        {
            var incentive = new Incentive(Apprenticeship.DateOfBirth, PlannedStartDate, paymentProfiles);
            if (!incentive.IsEligible)
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
                              DateTime.Now);

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

        public void CreatePayment(Guid pendingPaymentId, short collectionYear, byte collectionMonth)
        {
            var pendingPayment = GetPendingPayment(pendingPaymentId);
            if (!pendingPayment.IsValidated)
            {
                return;
            }

            RemoveExistingPaymentIfExists(pendingPaymentId);

            var paymentDate = DateTime.Now;

            AddPayment(pendingPaymentId, collectionYear, collectionMonth, pendingPayment, paymentDate);
            pendingPayment.SetPaymentMadeDate(paymentDate);
        }

        private void AddPayment(Guid pendingPaymentId, short collectionYear, byte collectionMonth, PendingPayment pendingPayment, DateTime paymentDate)
        {
            var payment = Payment.New(
                Guid.NewGuid(), 
                Model.Account, 
                Model.Id, 
                pendingPaymentId, 
                pendingPayment.Amount,
                paymentDate, 
                collectionYear, 
                collectionMonth);

            Model.PaymentModels.Add(payment.GetModel());
        }

        private void RemoveExistingPaymentIfExists(Guid pendingPaymentId)
        {
            var existingPayment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == pendingPaymentId);
            if (existingPayment != null)
            {
                Model.PaymentModels.Remove(existingPayment);
            }
        }

        private PendingPayment GetPendingPayment(Guid pendingPaymentId)
        {
            var pendingPayment = PendingPayments.SingleOrDefault(x => x.Id == pendingPaymentId);
            if (pendingPayment == null)
            {
                throw new ArgumentException("Pending payment does not exist.");
            }

            return pendingPayment;
		}

        public void ValidatePendingPaymentBankDetails(Guid pendingPaymentId, Accounts.Account account, CollectionPeriod collectionPeriod)
        {
            if (Account.Id != account.Id)
            {
                throw new InvalidPendingPaymentException($"Unable to validate PendingPayment {pendingPaymentId} of ApprenticeshipIncentive {Model.Id} because the provided Account record does not match the one against the incentive.");
            }

            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var legalEntity = account.GetLegalEntity(pendingPayment.Account.AccountLegalEntityId);

            var isValid = !string.IsNullOrEmpty(legalEntity.VrfVendorId);

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasBankDetails, isValid));
        }

        public void ValidateIsInLearning(Guid pendingPaymentId, Learner matchedLearner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var isInLearning = false;
            if (matchedLearner != null)
            {
                isInLearning = matchedLearner.SubmissionFound && matchedLearner.SubmissionData.IsInlearning == true;
            }

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.IsInLearning, isInLearning));
        }

        public void ValidateHasLearningRecord(Guid pendingPaymentId, Learner learner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var hasLearningRecord = false;

            if (learner != null && learner.SubmissionFound && learner.SubmissionData.LearningFoundStatus != null)
            {
                hasLearningRecord = learner.SubmissionData.LearningFoundStatus.LearningFound;
            }

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasLearningRecord, hasLearningRecord));
        }


        private PendingPayment GetPendingPaymentForValidationCheck(Guid pendingPaymentId)
        {
            var pendingPayment = PendingPayments.SingleOrDefault(x => x.Id == pendingPaymentId);
            if (pendingPayment == null)
            {
                throw new InvalidPendingPaymentException($"Unable to validate PendingPayment {pendingPaymentId} of ApprenticeshipIncentive {Model.Id} because the pending payment record does not exist.");
            }

            return pendingPayment;
        }

        private ApprenticeshipIncentive(Guid id, ApprenticeshipIncentiveModel model, bool isNew = false) : base(id, model, isNew)
        {
            if (isNew)
            {
                AddEvent(new Created
                {
                    ApprenticeshipIncentiveId = id,
                    AccountId = model.Account.Id,
                    ApprenticeshipId = model.Apprenticeship.Id
                });
            }
        }
    }
}
