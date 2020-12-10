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
        public DateTime PlannedStartDate => Model.PlannedStartDate;
        public IReadOnlyCollection<PendingPayment> PendingPayments => Model.PendingPaymentModels.Map().ToList().AsReadOnly();
        public PendingPayment NextDuePayment => GetNextDuePayment();
        public IReadOnlyCollection<Payment> Payments => Model.PaymentModels.Map().ToList().AsReadOnly();
        public bool PausePayments => Model.PausePayments;

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
        }

        public void CalculatePayments()
        {
            AddEvent(new PaymentsCalculationRequired(Model));
        }

        public void Delete()
        {
            if(Model.PaymentModels.Count > 0)
            {
                throw new DeleteIncentiveException("Cannot delete an incentive that has made a Payment");
            }
            IsDeleted = true;
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
            var age = Model.Apprenticeship.DateOfBirth.AgeOnThisDay(Model.PlannedStartDate);
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

            throw new InvalidIncentiveException("Cannot determine SubnominalCode as EmployerType has not been assigned as Levy or Non Levy");
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

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasIlrSubmission, learner.SubmissionFound));
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

        public void ValidateHasNoDataLocks(Guid pendingPaymentId, Learner matchedLearner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var hasDataLock = false;
            if (matchedLearner != null)
            {
                hasDataLock = matchedLearner.SubmissionFound && matchedLearner.SubmissionData.HasDataLock;
            }

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasNoDataLocks, !hasDataLock));
        }

        public void ValidateDaysInLearning(Guid pendingPaymentId, Learner matchedLearner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var hasEnoughDaysInLearning = false;
            if (matchedLearner != null)
            {
                hasEnoughDaysInLearning = PlannedStartDate.Date.AddDays(matchedLearner.GetDaysInLearning(collectionPeriod)) >= pendingPayment.DueDate.Date;
            }

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasDaysInLearning, hasEnoughDaysInLearning));
        }

        public void PauseSubsequentPayments(ServiceRequest serviceRequest)
        {
            if (Model.PausePayments == false)
            {
                Model.PausePayments = true;
                AddEvent(new PaymentsPaused(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship, serviceRequest));
            }
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
            if (!learner.SubmissionFound) return;

            ValidateHasLearningRecord(pendingPaymentId, learner, collectionPeriod);
            ValidateIsInLearning(pendingPaymentId, learner, collectionPeriod);
            ValidateHasNoDataLocks(pendingPaymentId, learner, collectionPeriod);
            ValidateDaysInLearning(pendingPaymentId, learner, collectionPeriod);
        }
    }
}
