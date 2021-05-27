using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Abstractions.Domain;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.EarningsResilienceCheck.Events;
using SFA.DAS.EmployerIncentives.Domain.Exceptions;
using SFA.DAS.EmployerIncentives.Domain.Extensions;
using SFA.DAS.EmployerIncentives.Domain.Interfaces;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PaymentsResumed = SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Events.PaymentsResumed;

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
        public bool PausePayments => Model.PausePayments;
        public IReadOnlyCollection<ClawbackPayment> Clawbacks => Model.ClawbackPaymentModels.Map().ToList().AsReadOnly();
        public IncentiveStatus Status => Model.Status;
        public AgreementVersion MinimumAgreementVersion => Model.MinimumAgreementVersion;
        private bool HasPaidEarnings => Model.PaymentModels.Any(p => p.PaidDate.HasValue);
        public IReadOnlyCollection<BreakInLearning> BreakInLearnings => Model.BreakInLearnings.ToList().AsReadOnly();
                
        internal static ApprenticeshipIncentive New(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime plannedStartDate, DateTime submittedDate, string submittedByEmail, AgreementVersion agreementVersion, IncentivePhase phase)
        {
            return new ApprenticeshipIncentive(
                id,
                new ApprenticeshipIncentiveModel
                {
                    Id = id,
                    ApplicationApprenticeshipId = applicationApprenticeshipId,
                    Account = account,
                    Apprenticeship = apprenticeship,
                    StartDate = plannedStartDate,
                    PausePayments = false,
                    SubmittedDate = submittedDate,
                    SubmittedByEmail = submittedByEmail,
                    Status = IncentiveStatus.Active,
                    MinimumAgreementVersion = agreementVersion,
                    Phase = phase
                }, true);
        }
        
        internal static ApprenticeshipIncentive Get(Guid id, ApprenticeshipIncentiveModel model)
        {
            return new ApprenticeshipIncentive(id, model);
        }

        public async Task CalculateEarnings(IIncentivePaymentProfilesService incentivePaymentProfilesService, ICollectionCalendarService collectionCalendarService)
        {
            if(Model.Status == IncentiveStatus.Withdrawn || Model.Status == IncentiveStatus.Stopped)
            {
                return;
            }

            var paymentProfiles = await incentivePaymentProfilesService.Get();
            var collectionCalendar = await collectionCalendarService.Get();

            var incentive = new Incentive(Apprenticeship.DateOfBirth, StartDate, paymentProfiles, Model.BreakInLearningDayCount);
            if (!incentive.IsEligible)
            {
                ClawbackAllPayments(collectionCalendar.GetActivePeriod());
                return;
            }

            foreach (var payment in incentive.Payments)
            {
                AddPendingPaymentsAndClawbackWhereRequired(payment, collectionCalendar);
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

        private void AddPendingPaymentsAndClawbackWhereRequired(ValueObjects.Payment payment, CollectionCalendar collectionCalendar)
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

            var existingPendingPayment = PendingPayments.SingleOrDefault(x => x.EarningType == pendingPayment.EarningType && !x.ClawedBack);
            if (existingPendingPayment == null)
            {
                Model.PendingPaymentModels.Add(pendingPayment.GetModel());
                return;
            }

            if (ExistingPendingPaymentHasBeenPaid(existingPendingPayment))
            {
                if (!existingPendingPayment.RequiresNewPayment(pendingPayment))
                {
                    return;
                }

                AddClawback(existingPendingPayment, collectionCalendar.GetActivePeriod());
                Model.PendingPaymentModels.Add(pendingPayment.GetModel());
                return;
            }

            RemoveUnpaidPaymentIfExists(existingPendingPayment);
            if (!existingPendingPayment.Equals(pendingPayment))
            {
                if(!existingPendingPayment.RequiresNewPaymentAfterBreakInLearning(Model.BreakInLearnings))
                {
                    return;
                }

                var existingPendingPaymentModel = existingPendingPayment.GetModel();
                if (Model.PendingPaymentModels.Remove(existingPendingPaymentModel))
                {
                    AddEvent(new PendingPaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, existingPendingPaymentModel));
                }
                Model.PendingPaymentModels.Add(pendingPayment.GetModel());
            }
        }     

        private void AddClawback(PendingPayment pendingPayment, CollectionPeriod collectionPeriod)
        {
            pendingPayment.ClawBack();
            var payment = Model.PaymentModels.Single(p => p.PendingPaymentId == pendingPayment.Id);

            if (!Model.ClawbackPaymentModels.Any(c => c.PendingPaymentId == pendingPayment.Id))
            {
                var clawback = ClawbackPayment.New(
                    Guid.NewGuid(),
                    Model.Account,
                    Model.Id,
                    pendingPayment.Id,
                    -pendingPayment.Amount,
                    DateTime.Now,
                    payment.SubnominalCode,
                    payment.Id);

                clawback.SetPaymentPeriod(collectionPeriod);

                Model.ClawbackPaymentModels.Add(clawback.GetModel());
            }
        }

        private void RemoveUnpaidPaymentIfExists(PendingPayment existingPendingPayment)
        {
            var existingPayment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == existingPendingPayment.Id);
            if (existingPayment != null)
            {
                if (Model.PaymentModels.Remove(existingPayment))
                {
                    AddEvent(new PaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, existingPayment));
                }
            }
        }

        private bool ExistingPendingPaymentHasBeenPaid(PendingPayment existingPendingPayment)
        {
            if (existingPendingPayment.PaymentMadeDate == null)
            {
                return false;
            }

            var existingPayment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == existingPendingPayment.Id);
            if (existingPayment == null || existingPayment.PaidDate == null)
            {
                return false;
            }

            return true;
        }

        private void ClawbackAllPayments(CollectionPeriod collectionPeriod)
        {
            RemoveUnpaidEarnings();
            ClawbackPayments(PendingPayments, collectionPeriod);
        }

        public void CalculatePayments()
        {
            AddEvent(new PaymentsCalculationRequired(Model));
        }
        
        public async Task Withdraw(ICollectionCalendarService collectionCalendarService)
        {
            Model.Status = IncentiveStatus.Withdrawn;
            if (HasPaidEarnings)
            {
                var calendarService = await collectionCalendarService.Get();
                ClawbackAllPayments(calendarService.GetActivePeriod());
                Model.PausePayments = false;
            }
            else
            {
                RemoveUnpaidEarnings();
            }
        }

        public void CreatePayment(Guid pendingPaymentId, short collectionYear, byte collectionPeriod)
        {
            var pendingPayment = GetPendingPayment(pendingPaymentId);
            if (!pendingPayment.IsValidated(collectionYear, collectionPeriod))
            {
                return;
            }

            var paymentDate = DateTime.Today;

            var existingPayment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == pendingPaymentId);
            if (existingPayment != null)
            {
                existingPayment.CalculatedDate = paymentDate;
                existingPayment.PaymentPeriod = collectionPeriod;
                existingPayment.PaymentYear = collectionYear;
                existingPayment.SubnominalCode = DetermineSubnominalCode();
                existingPayment.Amount = pendingPayment.Amount;
            }
            else
            {
                AddPayment(pendingPaymentId, collectionYear, collectionPeriod, pendingPayment, paymentDate);
            }
            
            pendingPayment.SetPaymentMadeDate(paymentDate);
        }

        public void SetStartDate(DateTime startDate)
        {
            Model.StartDate = startDate;
        }

        public async Task SetChangeOfCircumstances(Learner learner, ICollectionCalendarService collectionCalendarService)
        {
            if (Id != learner.ApprenticeshipIncentiveId)
            {
                throw new InvalidOperationException();
            }

            if (learner.SubmissionData.SubmissionFound)
            {
                if (learner.SubmissionData.LearningData.StartDate.HasValue)
                {
                    SetStartDateChangeOfCircumstance(learner.SubmissionData.LearningData.StartDate.Value);
                }

                SetBreakInLearningDayCount(learner.GetBreakInLearningDayCount());
                await SetLearningStoppedChangeOfCircumstance(learner.SubmissionData.LearningData.StoppedStatus, collectionCalendarService);                
            }

            SetHasPossibleChangeOfCircumstances(false);
        }

        public void SetHasPossibleChangeOfCircumstances(bool hasPossibleChangeOfCircumstances)
        {
            Model.HasPossibleChangeOfCircumstances = hasPossibleChangeOfCircumstances;
        }

        private void SetStartDateChangeOfCircumstance(DateTime startDate)
        {
            var previousStartDate = Model.StartDate;
            SetStartDate(startDate);
            if (previousStartDate != Model.StartDate)
            {
                AddEvent(new StartDateChanged(
                    Model.Id,
                    previousStartDate,
                    Model.StartDate,
                    Model));

                SetMinimumAgreementVersion(startDate);
            }
        }

        private void SetMinimumAgreementVersion(DateTime startDate)
        {
            var existingMinimumAgreementVersion = Model.MinimumAgreementVersion;
            Model.MinimumAgreementVersion = Model.MinimumAgreementVersion.ChangedStartDate(startDate);
            if (existingMinimumAgreementVersion != Model.MinimumAgreementVersion)
            {
                AddEvent(new MinimumAgreementVersionChanged(
                Model.Id,
                existingMinimumAgreementVersion.MinimumRequiredVersion,
                Model.MinimumAgreementVersion.MinimumRequiredVersion.Value,
                Model));
            }
        }
        private void StartBreakInLearning(DateTime startDate)
        {
            if(Model.BreakInLearnings.Any(b => b.StartDate == startDate.Date))
            {
                return;
            }

            Model.BreakInLearnings.Add(new BreakInLearning(startDate));
        }

        private void StopBreakInLearning(DateTime stopDate)
        {
            if (Model.BreakInLearnings.Any(b => b.EndDate == stopDate.Date) || Model.BreakInLearnings.Count == 0)
            {
                return;
            }

            Model.BreakInLearnings.Single(b => !b.EndDate.HasValue).SetEndDate(stopDate);            
        }

        private async Task SetLearningStoppedChangeOfCircumstance(LearningStoppedStatus learningStoppedStatus, ICollectionCalendarService collectionCalendarService)
        {
            if (learningStoppedStatus.LearningStopped && Model.Status != IncentiveStatus.Stopped)
            {
                Model.Status = IncentiveStatus.Stopped;
                StartBreakInLearning(learningStoppedStatus.DateStopped.Value);
                await RemoveEarningsAfterStopDate(learningStoppedStatus.DateStopped.Value, collectionCalendarService);
                AddEvent(new LearningStopped(
                    Model.Id,
                    learningStoppedStatus.DateStopped.Value));
            }
            else if(Model.Status == IncentiveStatus.Stopped && !learningStoppedStatus.LearningStopped)
            {
                Model.Status = IncentiveStatus.Active;
                StopBreakInLearning(learningStoppedStatus.DateResumed.Value.AddDays(-1));
                AddEvent(new LearningResumed(
                   Model.Id,
                   learningStoppedStatus.DateResumed.Value));
            }            
        }

        private void SetBreakInLearningDayCount(int breakInLearningDayCount)
        {
            Model.BreakInLearningDayCount = breakInLearningDayCount;
        }

        private async Task RemoveEarningsAfterStopDate(DateTime dateStopped, ICollectionCalendarService collectionCalendarService)
        {
            var collectionCalendar = await collectionCalendarService.Get();

            RemoveUnpaidEarnings(Model.PendingPaymentModels.Where(x => x.DueDate >= dateStopped));
            ClawbackPayments(PendingPayments.Where(x => x.DueDate >= dateStopped), collectionCalendar.GetActivePeriod());
        }

        private void ClawbackPayments(IEnumerable<PendingPayment> pendingPayments, CollectionPeriod collectionPeriod)
        {
            foreach (var paidPendingPayment in pendingPayments)
            {
                AddClawback(paidPendingPayment, collectionPeriod);
            }
        }

        private void RemoveUnpaidEarnings()
        {
            RemoveUnpaidEarnings(Model.PendingPaymentModels);
        }

        private void RemoveUnpaidEarnings(IEnumerable<PendingPaymentModel> pendingPayments)
        {
            pendingPayments.Where(x => x.PaymentMadeDate == null).ToList()
                .ForEach(pp => {
                    if (Model.PendingPaymentModels.Remove(pp))
                    {
                        AddEvent(new PendingPaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, pp));
                    }
                });

            var pendingPaymentsToDelete = new List<PendingPaymentModel>();
            foreach (var paidPendingPayment in pendingPayments.Where(x => x.PaymentMadeDate != null))
            {
                var payment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == paidPendingPayment.Id);
                if (payment != null && payment.PaidDate == null)
                {
                    if(Model.PaymentModels.Remove(payment))
                    {
                        AddEvent(new PaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, payment));
                    }
                    pendingPaymentsToDelete.Add(paidPendingPayment);
                }
            }

            foreach (var deletedPendingPayment in pendingPaymentsToDelete)
            {
                if (Model.PendingPaymentModels.Remove(deletedPendingPayment))
                {
                    AddEvent(new PendingPaymentDeleted(Model.Account.Id, Model.Account.AccountLegalEntityId, Model.Apprenticeship.UniqueLearnerNumber, deletedPendingPayment));
                }
            }
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

            if (legalEntity == null)
            {
                pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasBankDetails, false));
                return;
            }

            var isValid = !string.IsNullOrEmpty(legalEntity.VrfVendorId);

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasBankDetails, isValid));
        }

        public void ValidatePaymentsNotPaused(Guid pendingPaymentId, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var isValid = !Model.PausePayments;

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.PaymentsNotPaused, isValid));
        }

        public void ValidateMinimumRequiredAgreementVersion(Guid pendingPaymentId, Accounts.Account account, CollectionPeriod collectionPeriod)
        {
            if (account == null || Account.Id != account.Id)
            {
                throw new InvalidPendingPaymentException($"Unable to validate PendingPayment {pendingPaymentId} of ApprenticeshipIncentive {Model.Id} because the provided Account record does not match the one against the incentive.");
            }

            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);
                        
            var legalEntity = account.GetLegalEntity(pendingPayment.Account.AccountLegalEntityId);

            var isValid = legalEntity.SignedAgreementVersion >= MinimumAgreementVersion.MinimumRequiredVersion;

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasSignedMinVersion, isValid));
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

        public void PauseSubsequentPayments(ServiceRequest serviceRequest)
        {
            if (!Model.PausePayments)
            {
                Model.PausePayments = true;
                Model.Status = IncentiveStatus.Paused;
                AddEvent(new PaymentsPaused(Model.Account.Id, Model.Account.AccountLegalEntityId, Model, serviceRequest));
            }
            else
            {
                throw new PausePaymentsException("Payments already paused");
            }
        }

        public void ResumePayments(ServiceRequest serviceRequest)
        {
            if (Model.PausePayments)
            {
                Model.PausePayments = false;
                Model.Status = IncentiveStatus.Active;
                AddEvent(new PaymentsResumed(Model.Account.Id, Model.Account.AccountLegalEntityId, Model, serviceRequest));
            }
            else
            {
                throw new PausePaymentsException("Payments are not paused");
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
                model.Status = IncentiveStatus.Active;
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
