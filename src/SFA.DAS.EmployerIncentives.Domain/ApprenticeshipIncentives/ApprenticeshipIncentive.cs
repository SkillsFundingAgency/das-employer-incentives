using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives
{
    public sealed class ApprenticeshipIncentive : AggregateRoot<Guid, ApprenticeshipIncentiveModel>
    {
        public Account Account => Model.Account;
        public Apprenticeship Apprenticeship => Model.Apprenticeship;
        public DateTime StartDate => Model.StartDate;
        public bool RefreshedLearnerForEarnings => Model.RefreshedLearnerForEarnings;
        public bool HasPossibleChangeOfCircumstances => Model.HasPossibleChangeOfCircumstances;
        public IReadOnlyCollection<PendingPayment> PendingPayments => Model.PendingPaymentModels.Map().ToList().AsReadOnly();
        public PendingPayment NextDuePayment => GetNextDuePayment();
        public IReadOnlyCollection<Payment> Payments => Model.PaymentModels.Map().ToList().AsReadOnly();

        internal static ApprenticeshipIncentive New(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime startDate)
        {
            return new ApprenticeshipIncentive(
                id,
                new ApprenticeshipIncentiveModel
                {
                    Id = id,
                    ApplicationApprenticeshipId = applicationApprenticeshipId,
                    Account = account,
                    Apprenticeship = apprenticeship,
                    StartDate = startDate
                }, true);
        }

        internal static ApprenticeshipIncentive Get(Guid id, ApprenticeshipIncentiveModel model)
        {
            return new ApprenticeshipIncentive(id, model);
        }

        public void CalculateEarnings(IEnumerable<IncentivePaymentProfile> paymentProfiles, CollectionCalendar collectionCalendar)
        {
            if (Model.PendingPaymentModels.Any())
            {
                return;
            }

            var incentive = new Incentive(Apprenticeship.DateOfBirth, StartDate, paymentProfiles);
            if (!incentive.IsEligible)
            {
                return;
            }

            foreach (var payment in incentive.Payments)
            {
                var pendingPayment = PendingPayment.New(
                              Guid.NewGuid(),
                              Model.Account,
                              Model.Id,
                              payment.Amount,
                              payment.PaymentDate,
                              DateTime.Now,
                              payment.EarningType);

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

            Model.RefreshedLearnerForEarnings = false;
        }

        public void CalculatePayments()
        {
            AddEvent(new PaymentsCalculationRequired(Model));
        }

        public void CreatePayment(Guid pendingPaymentId, short collectionYear, byte collectionPeriod)
        {
            var pendingPayment = GetPendingPayment(pendingPaymentId);
            if (!pendingPayment.IsValidated(collectionYear, collectionPeriod))
            {
                return;
            }

            RemoveExistingPaymentIfExists(pendingPaymentId);

            var paymentDate = DateTime.Today;

            AddPayment(pendingPaymentId, collectionYear, collectionPeriod, pendingPayment, paymentDate);
            pendingPayment.SetPaymentMadeDate(paymentDate);
        }

        public void SetStartDate(DateTime startDate)
        {
            if (startDate != Model.StartDate)
            {
                Model.StartDate = startDate;
                Model.PendingPaymentModels.Clear();
            }
        }

        public void SetHasPossibleChangeOfCircumstances(bool hasPossibleChangeOfCircumstances)
        {
            Model.HasPossibleChangeOfCircumstances = hasPossibleChangeOfCircumstances;
        }

        private void AddPayment(Guid pendingPaymentId, short collectionYear, byte collectionPeriod, PendingPayment pendingPayment, DateTime paymentDate)
        {
            var subnominalCode = DetermineSubnominalCode();

            var payment = Payment.New(
                Guid.NewGuid(),
                Model.Account,
                Model.Id,
                pendingPaymentId,
                pendingPayment.Amount,
                paymentDate, 
                collectionYear,
                collectionPeriod,
                subnominalCode);

            Model.PaymentModels.Add(payment.GetModel());
        }

        private SubnominalCode DetermineSubnominalCode()
        {
            var age = Model.Apprenticeship.DateOfBirth.AgeOnThisDay(Model.StartDate);
            var employerType = Model.Apprenticeship.EmployerType;

            if (employerType == ApprenticeshipEmployerType.Levy)
            {
                if (age <= 18)
                {
                    return SubnominalCode.Levy16To18;
                }

                return SubnominalCode.Levy19Plus;
            }

            if (employerType == ApprenticeshipEmployerType.NonLevy)
            {
                if (age <= 18)
                {
                    return SubnominalCode.NonLevy16To18;
                }

                return SubnominalCode.NonLevy19Plus;
            }

            throw new ArgumentException("Cannot determine SubnominalCode as EmployerType has not been assigned as Levy or Non Levy");
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

        private void ValidateSubmissionFound(Guid pendingPaymentId, Learner learner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasIlrSubmission, learner.SubmissionData.SubmissionFound));
        }

        public void ValidateIsInLearning(Guid pendingPaymentId, Learner matchedLearner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var isInLearning = false;
            if (matchedLearner != null)
            {
                isInLearning = matchedLearner.SubmissionData.SubmissionFound && matchedLearner.SubmissionData.LearningData.IsInlearning == true;
            }

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.IsInLearning, isInLearning));
        }

        public void ValidateHasLearningRecord(Guid pendingPaymentId, Learner learner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var hasLearningRecord = false;

            if (learner != null && learner.SubmissionData.SubmissionFound)
            {
                hasLearningRecord = learner.SubmissionData.LearningData.LearningFound;
            }

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasLearningRecord, hasLearningRecord));
        }

        public void ValidateHasNoDataLocks(Guid pendingPaymentId, Learner matchedLearner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var hasDataLock = false;
            if (matchedLearner != null)
            {
                hasDataLock = matchedLearner.SubmissionData.SubmissionFound && matchedLearner.SubmissionData.LearningData.HasDataLock == true;
            }

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasNoDataLocks, !hasDataLock));
        }

        public void LearnerRefreshCompleted()
        {
            Model.RefreshedLearnerForEarnings = true;
		}

        public void ValidateDaysInLearning(Guid pendingPaymentId, Learner matchedLearner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var hasEnoughDaysInLearning = false;
            if (matchedLearner != null)
            {
                hasEnoughDaysInLearning = StartDate.Date.AddDays(matchedLearner.GetDaysInLearning(collectionPeriod)) >= pendingPayment.DueDate.Date;
            }

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasDaysInLearning, hasEnoughDaysInLearning));
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

        private PendingPayment GetNextDuePayment()
        {
            var next = Model.PendingPaymentModels
                .Where(pp => pp.PaymentMadeDate == null && pp.PaymentYear.HasValue && pp.PeriodNumber.HasValue)
                .OrderBy(pp => pp.DueDate).FirstOrDefault();

            return next?.Map();
        }

        public void ValidateLearningData(Guid pendingPaymentId, Learner learner, CollectionPeriod collectionPeriod)
        {
            ValidateSubmissionFound(pendingPaymentId, learner, collectionPeriod);
            if (!learner.SubmissionData.SubmissionFound) return;

            ValidateHasLearningRecord(pendingPaymentId, learner, collectionPeriod);
            ValidateIsInLearning(pendingPaymentId, learner, collectionPeriod);
            ValidateHasNoDataLocks(pendingPaymentId, learner, collectionPeriod);
            ValidateDaysInLearning(pendingPaymentId, learner, collectionPeriod);
        }
    }
}
