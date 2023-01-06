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
using SFA.DAS.EmployerIncentives.Domain.ValueObjects.Earnings;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IReadOnlyCollection<BreakInLearning> BreakInLearnings => Model.BreakInLearnings.OrderBy(b => b.StartDate).ToList().AsReadOnly();
        public IncentivePhase Phase => Model.Phase;
        public WithdrawnBy? WithdrawnBy => Model.WithdrawnBy;
        public DateTime SubmissionDate => Model.SubmittedDate.Value;
        public IReadOnlyCollection<EmploymentCheck> EmploymentChecks => Model.EmploymentCheckModels.Map().ToList().AsReadOnly();
        public IReadOnlyCollection<ValidationOverride> ValidationOverrides => Model.ValidationOverrideModels.Map().ToList().AsReadOnly();

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

        public void CalculateEarnings(CollectionCalendar collectionCalendar)
        {
            CalculateEarnings(collectionCalendar, null);
        }

        public void ReCalculateEarnings(CollectionCalendar collectionCalendar)
        {
            var calculator = EarningsCalculator.Create(
                this,
                collectionCalendar,
                e => AddEvent(e));

            calculator.ReCalculate();
        }

        private void CalculateEarnings(CollectionCalendar collectionCalendar, Learner learner)
        {
            var calculator = EarningsCalculator.Create(
                this,
                collectionCalendar,
                e => AddEvent(e),
                learner);

            calculator.Calculate();
        }

        public void CalculatePayments()
        {
            AddEvent(new PaymentsCalculationRequired(Model));
        }

        private void ChangeStatus(IncentiveStatus newStatus)
        {
            Model.PreviousStatus = Model.Status;
            Model.Status = newStatus;
        }

        public void Withdraw(WithdrawnBy withdrawnBy, CollectionCalendar collectionCalendar)
        {
            ChangeStatus(IncentiveStatus.Withdrawn);

            CalculateEarnings(collectionCalendar);

            Model.WithdrawnBy = withdrawnBy;
        }

        public void Reinstate(CollectionCalendar collectionCalendar)
        {
            Model.WithdrawnBy = null;
            Model.PausePayments = true;
            ChangeStatus(IncentiveStatus.Paused);
            CalculateEarnings(collectionCalendar);
        }

        public void CreatePayment(Guid pendingPaymentId, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPayment(pendingPaymentId);
            if (!pendingPayment.IsValidated(collectionPeriod))
            {
                return;
            }

            var paymentDate = DateTime.Today;

            var existingPayment = Model.PaymentModels.SingleOrDefault(x => x.PendingPaymentId == pendingPaymentId);
            if (existingPayment != null)
            {
                existingPayment.CalculatedDate = paymentDate;
                existingPayment.PaymentPeriod = collectionPeriod.PeriodNumber;
                existingPayment.PaymentYear = collectionPeriod.AcademicYear;
                existingPayment.SubnominalCode = DetermineSubnominalCode();
                existingPayment.Amount = pendingPayment.Amount;
            }
            else
            {
                AddPayment(pendingPaymentId, collectionPeriod, pendingPayment, paymentDate);
            }

            pendingPayment.SetPaymentMadeDate(paymentDate);
        }

        public void SetStartDate(DateTime startDate)
        {
            Model.StartDate = startDate;
        }

        public void SetHasPossibleChangeOfCircumstances(bool hasPossibleChangeOfCircumstances)
        {
            Model.HasPossibleChangeOfCircumstances = hasPossibleChangeOfCircumstances;
        }

        public void SetStartDateChangeOfCircumstance(
            DateTime startDate,
            CollectionCalendar collectionCalendar,
            IDateTimeService dateTimeService)
        {
            var previousStartDate = Model.StartDate;
            SetStartDate(startDate);
            if (previousStartDate != Model.StartDate)
            {
                CalculateEarnings(collectionCalendar);

                AddEvent(new StartDateChanged(
                    Model.Id,
                    previousStartDate,
                    Model.StartDate,
                    Model));

                SetMinimumAgreementVersion(startDate);
                RefreshEmploymentChecks(dateTimeService);
            }
        }

        private void SetMinimumAgreementVersion(DateTime startDate)
        {
            var existingMinimumAgreementVersion = Model.MinimumAgreementVersion;
            Model.MinimumAgreementVersion = Model.MinimumAgreementVersion.ChangedStartDate(Phase.Identifier, startDate);
            if (existingMinimumAgreementVersion != Model.MinimumAgreementVersion)
            {
                AddEvent(new MinimumAgreementVersionChanged(
                Model.Id,
                existingMinimumAgreementVersion.MinimumRequiredVersion,
                Model.MinimumAgreementVersion.MinimumRequiredVersion.Value,
                Model));
            }
        }

        public void SetBreaksInLearning(
            IList<LearningPeriod> periods,
            CollectionCalendar collectionCalendar)
        {
            var breaks = new List<BreakInLearning>();
            for (var i = 0; i < periods.Count - 1; i++)
            {
                var start = periods[i].EndDate.AddDays(1);
                var end = periods[i + 1].StartDate.AddDays(-1);

                breaks.Add(BreakInLearning.Create(start, end));
            }

            if (breaks.SequenceEqual(BreakInLearnings)) return;

            Model.BreakInLearnings = breaks;

            CalculateEarnings(collectionCalendar);
        }   

        private void StartBreakInLearning(DateTime startDate)
        {
            if (Model.BreakInLearnings.Any(b => b.StartDate == startDate.Date && !b.EndDate.HasValue)) return;
            Model.BreakInLearnings.Add(new BreakInLearning(startDate));
        }

        public void SetLearningStoppedChangeOfCircumstance(
            Learner learner,
            CollectionCalendar collectionCalendar)
        {
            var stoppedStatus = learner.StoppedStatus;

            if (stoppedStatus.LearningStopped && Model.Status != IncentiveStatus.Stopped)
            {
                ChangeStatus(IncentiveStatus.Stopped);
                StartBreakInLearning(stoppedStatus.DateStopped.Value);
                
                CalculateEarnings(collectionCalendar, learner);

                AddEvent(new LearningStopped(
                    Model.Id,
                    stoppedStatus.DateStopped.Value));
            }
            else if (Model.Status == IncentiveStatus.Stopped &&
                !stoppedStatus.LearningStopped &&
                stoppedStatus.DateResumed != null)
            {
                ChangeStatus(IncentiveStatus.Active);

                AddEvent(new BreakInLearningDeleted(Model.Id));

                RemoveAll365Checks();
                CalculateEarnings(collectionCalendar, learner);

                AddEvent(new LearningResumed(
                    Model.Id,
                    stoppedStatus.DateResumed.Value));
            }
            else if (Model.Status == IncentiveStatus.Stopped && stoppedStatus.LearningStopped)
            {
                ChangeStatus(IncentiveStatus.Stopped);
                CalculateEarnings(collectionCalendar, learner);

                AddEvent(new LearningStopped(
                    Model.Id,
                    stoppedStatus.DateStopped.Value));
            }
        }

        private void AddPayment(Guid pendingPaymentId, CollectionPeriod collectionPeriod, PendingPayment pendingPayment, DateTime paymentDate)
        {
            var subnominalCode = DetermineSubnominalCode();

            var payment = Payment.New(
                Guid.NewGuid(),
                Model.Account,
                Model.Id,
                pendingPaymentId,
                pendingPayment.Amount,
                paymentDate,
                collectionPeriod.AcademicYear,
                collectionPeriod.PeriodNumber,
                subnominalCode,
                string.Empty);

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

        public void ValidateEmploymentChecks(IDateTimeService dateTimeService, Guid pendingPaymentId, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            ValidateEmployedAtStartOfApprenticeship(collectionPeriod, pendingPayment);

            ValidateNotEmployedBeforeSchemeStartDate(collectionPeriod, pendingPayment);

            ValidateEmployedAt365Days(dateTimeService, collectionPeriod, pendingPayment);
        } 

        public void ValidatePaymentsNotBlockedForAccountLegalEntity(Guid pendingPaymentId, Accounts.Account account, CollectionPeriod collectionPeriod)
        {
            if (Account.Id != account.Id)
            {
                throw new InvalidPendingPaymentException($"Unable to validate PendingPayment {pendingPaymentId} of ApprenticeshipIncentive {Model.Id} because the provided Account record does not match the one against the incentive.");
            }

            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var legalEntity = account.GetLegalEntity(pendingPayment.Account.AccountLegalEntityId);

            if (legalEntity == null)
            {
                throw new InvalidPendingPaymentException($"Unable to validate PendingPayment {pendingPaymentId} of ApprenticeshipIncentive {Model.Id} because the provided account legal entity does not exist.");
            }

            var isValid = (!legalEntity.VendorBlockEndDate.HasValue || legalEntity.VendorBlockEndDate < DateTime.Now);

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.BlockedForPayments, isValid));
        }
        
        private void ValidateNotEmployedBeforeSchemeStartDate(CollectionPeriod collectionPeriod, PendingPayment pendingPayment)
        {
            var employedBeforeSchemeStartedCheck = EmploymentChecks.FirstOrDefault(x =>
                x.CheckType == EmploymentCheckType.EmployedBeforeSchemeStarted);

            var employedBeforeSchemeStartedResult = employedBeforeSchemeStartedCheck?.Result != null &&
                                                    !employedBeforeSchemeStartedCheck.Result.Value;

            pendingPayment.AddValidationResult(
                PendingPaymentValidationResult.New(
                    Guid.NewGuid(),
                    collectionPeriod,
                    ValidationStep.EmployedBeforeSchemeStarted,
                    employedBeforeSchemeStartedResult,
                    GetOverrideStep(ValidationStep.EmployedBeforeSchemeStarted)));
        }

        private void ValidateEmployedAtStartOfApprenticeship(CollectionPeriod collectionPeriod, PendingPayment pendingPayment)
        {
            var employedAtStartOfApprenticeshipCheck = EmploymentChecks.FirstOrDefault(x =>
                x.CheckType == EmploymentCheckType.EmployedAtStartOfApprenticeship);

            var employedAtStartOfApprenticeshipResult = employedAtStartOfApprenticeshipCheck?.Result != null &&
                                                        employedAtStartOfApprenticeshipCheck.Result.Value;

            pendingPayment.AddValidationResult(
                PendingPaymentValidationResult.New(
                    Guid.NewGuid(),
                    collectionPeriod,
                    ValidationStep.EmployedAtStartOfApprenticeship,
                    employedAtStartOfApprenticeshipResult,
                    GetOverrideStep(ValidationStep.EmployedAtStartOfApprenticeship)));
        }

        private void ValidateEmployedAt365Days(IDateTimeService dateTimeService, CollectionPeriod collectionPeriod, PendingPayment pendingPayment)
        {
            if (pendingPayment.EarningType  == EarningType.FirstPayment)
            {
                return;
            }

            var employedAt365DaysFirstCheck = EmploymentChecks.FirstOrDefault(x =>
                x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck);

            var employedAt365DaysFirstCheckResult = employedAt365DaysFirstCheck?.Result != null &&
                                                        employedAt365DaysFirstCheck.Result.Value;

            bool validationResult = true;

            if (employedAt365DaysFirstCheck == null &&
               pendingPayment.DueDate.AddDays(21) > dateTimeService.UtcNow().Date)
            {
                validationResult = false;
            }
            
            pendingPayment.AddValidationResult(
                PendingPaymentValidationResult.New(
                    Guid.NewGuid(),
                    collectionPeriod,
                    ValidationStep.EmployedAt365Days,
                    validationResult,
                    GetOverrideStep(ValidationStep.EmployedAt365Days)));

            if (!employedAt365DaysFirstCheckResult)
            {
                var employedAt365DaysSecondCheck = EmploymentChecks.FirstOrDefault(x =>
                    x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck);

                var employedAt365DaysSecondCheckResult = employedAt365DaysSecondCheck?.Result != null &&
                                                         employedAt365DaysSecondCheck.Result.Value;

                pendingPayment.AddValidationResult(
                    PendingPaymentValidationResult.New(
                        Guid.NewGuid(),
                        collectionPeriod,
                        ValidationStep.EmployedAt365Days,
                        employedAt365DaysSecondCheckResult,
                        GetOverrideStep(ValidationStep.EmployedAt365Days)));
            }
        }

        private void ValidateSubmissionFound(Guid pendingPaymentId, Learner learner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.HasIlrSubmission, learner.SubmissionData.SubmissionFound));
        }

        private void ValidateLearnerMatchSuccessful(Guid pendingPaymentId, Learner learner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            pendingPayment.AddValidationResult(PendingPaymentValidationResult.New(Guid.NewGuid(), collectionPeriod, ValidationStep.LearnerMatchSuccessful, learner.SuccessfulLearnerMatch));
        }

        public void ValidateIsInLearning(Guid pendingPaymentId, Learner matchedLearner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var isInLearning = false;
            if (matchedLearner != null)
            {
                isInLearning = matchedLearner.SubmissionData.SubmissionFound && matchedLearner.SubmissionData.LearningData.IsInlearning == true;
            }

            pendingPayment.AddValidationResult(
                PendingPaymentValidationResult.New(
                    Guid.NewGuid(),
                    collectionPeriod,
                    ValidationStep.IsInLearning,
                    isInLearning,
                    GetOverrideStep(ValidationStep.IsInLearning)));
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

            pendingPayment.AddValidationResult(
                PendingPaymentValidationResult.New(
                    Guid.NewGuid(),
                    collectionPeriod,
                    ValidationStep.HasNoDataLocks,
                    !hasDataLock,
                    GetOverrideStep(ValidationStep.HasNoDataLocks)));
        }

        private void LearnerRefreshCompleted()
        {
            Model.RefreshedLearnerForEarnings = true;
        }

        public void ValidateDaysInLearning(Guid pendingPaymentId, Learner matchedLearner, CollectionPeriod collectionPeriod)
        {
            var pendingPayment = GetPendingPaymentForValidationCheck(pendingPaymentId);

            var hasEnoughDaysInLearning = false;
            if (matchedLearner != null)
            {
                var incentive = Incentive.Create(this);
                hasEnoughDaysInLearning = matchedLearner.GetDaysInLearning(collectionPeriod) > incentive.MinimumDaysInLearning(pendingPayment.EarningType);
            }

            pendingPayment.AddValidationResult(
                PendingPaymentValidationResult.New(
                    Guid.NewGuid(),
                    collectionPeriod,
                    ValidationStep.HasDaysInLearning,
                    hasEnoughDaysInLearning,
                    GetOverrideStep(ValidationStep.HasDaysInLearning)));
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
                .Where(pp => pp.PaymentMadeDate == null && pp.CollectionPeriod != null)
                .OrderBy(pp => pp.DueDate).FirstOrDefault();

            return next?.Map();
        }

        public void ValidateLearningData(Guid pendingPaymentId, Learner learner, CollectionPeriod collectionPeriod)
        {
            ValidateLearnerMatchSuccessful(pendingPaymentId, learner, collectionPeriod);
            if (!learner.SuccessfulLearnerMatch) return;

            ValidateSubmissionFound(pendingPaymentId, learner, collectionPeriod);
            if (!learner.SubmissionData.SubmissionFound) return;

            ValidateHasLearningRecord(pendingPaymentId, learner, collectionPeriod);
            ValidateIsInLearning(pendingPaymentId, learner, collectionPeriod);
            ValidateHasNoDataLocks(pendingPaymentId, learner, collectionPeriod);
            ValidateDaysInLearning(pendingPaymentId, learner, collectionPeriod);
        }

        public void UpdateEmploymentCheck(EmploymentCheckResult checkResult)
        {
            var employmentCheck = Model.EmploymentCheckModels.SingleOrDefault(c => c.CorrelationId == checkResult.CorrelationId);
            if (employmentCheck == null)
            {
                return; // ignore superseded results
            }

            if (employmentCheck.ResultDateTime.HasValue && employmentCheck.ResultDateTime > checkResult.DateChecked)
            {
                return; // ignore older changes
            }

            employmentCheck.ResultDateTime = checkResult.DateChecked;

            switch (checkResult.Result)
            {
                case EmploymentCheckResultType.Employed:
                    employmentCheck.Result = true;
                    break;

                case EmploymentCheckResultType.NotEmployed:
                    employmentCheck.Result = false;
                    break;

                case EmploymentCheckResultType.Error:
                    {
                        employmentCheck.Result = null;
                        if (checkResult.ErrorType.HasValue)
                        {
                            employmentCheck.ErrorType = checkResult.ErrorType;
                        }
                        else
                        {
                            throw new InvalidEmploymentCheckErrorTypeException("Value not set");
                        }
                    }
                    break;
            }
        }      

        public void RefreshEmploymentChecks(IDateTimeService dateTimeService, ServiceRequest serviceRequest = null, string checkType = null)
        {
            if (checkType == RefreshEmploymentCheckType.EmployedAt365DaysCheck.ToString())
            {
                if (!HasSuccessfulChecks(new List<EmploymentCheckType> {
                        EmploymentCheckType.EmployedAtStartOfApprenticeship,
                        EmploymentCheckType.EmployedBeforeSchemeStarted
                    }))
                {
                    throw new InvalidOperationException("Employed at 365 days check cannot be refreshed if initial employment checks have not completed");
                }

                if (!CanRefreshEmploymentCheck(EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck))
                {
                    throw new InvalidOperationException("Employed at 365 days check cannot be refreshed if 365 day employment checks have not previously executed");
                }

                Model.EmploymentCheckModels.Where(x => x.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck).ToList()
                    .ForEach(ec =>
                    {
                        if (Model.EmploymentCheckModels.Remove(ec))
                        {
                            AddEvent(new EmploymentCheckDeleted(ec));
                        }
                    });
            }
            else
            {
                Model.EmploymentCheckModels.ToList()
                    .ForEach(ec =>
                    {
                        if (Model.EmploymentCheckModels.Remove(ec))
                        {
                            AddEvent(new EmploymentCheckDeleted(ec));
                        }
                    });
            }

            AddEmploymentChecks(dateTimeService, serviceRequest);
        }

        public void AddEmploymentChecks(IDateTimeService dateTimeService, ServiceRequest serviceRequest = null)
        {
            if (Status == IncentiveStatus.Withdrawn)
            {
                return;
            }

            var secondPaymentDueDate = Model.PendingPaymentModels.SingleOrDefault(pp => pp.EarningType == EarningType.SecondPayment && !pp.ClawedBack)?.DueDate;

            if (secondPaymentDueDate != null &&
                secondPaymentDueDate.Value.AddDays(21).Date <= dateTimeService.UtcNow().Date && 
                HasSuccessfulChecks(new List<EmploymentCheckType> { EmploymentCheckType.EmployedAtStartOfApprenticeship, EmploymentCheckType.EmployedBeforeSchemeStarted }))
            {
                var existingFirstCheck = Model.EmploymentCheckModels.SingleOrDefault(ec => ec.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck);
                var existingSecondCheck = Model.EmploymentCheckModels.SingleOrDefault(ec => ec.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck);

                if (existingFirstCheck == null && existingSecondCheck == null) // check never performed
                {
                    AddEmployedAt365PaymentDueDateFirstCheck(serviceRequest: serviceRequest);
                }
                else if (existingSecondCheck == null && existingFirstCheck.Result.HasValue && existingFirstCheck.Result.Value) // first check succeeded
                {
                    return;
                }
                else if(existingFirstCheck != null && (existingFirstCheck.Result.HasValue && !existingFirstCheck.Result.Value)
                        || (existingFirstCheck != null && !existingFirstCheck.Result.HasValue && existingFirstCheck.ErrorType != null)) // first check failed or returned error code
                {
                    if (secondPaymentDueDate.Value.AddDays(42).Date <= dateTimeService.UtcNow().Date)
                    {
                        AddEmployedAt365PaymentDueDateSecondCheck(serviceRequest: serviceRequest);
                    }
                }

                return;
            }

            if (StartDate.AddDays(42).Date > dateTimeService.UtcNow().Date) // has not started 6 weeks ago
            {   
                return;
            }

            if (EmploymentChecks.Any()) // already requested
            {
                return;
            }

            AddEmploymentBeforeSchemeCheck(serviceRequest: serviceRequest);
            AddEmployedAtStartOfApprenticeshipCheck(serviceRequest: serviceRequest);            
        }
        

        private void AddEmployedAtStartOfApprenticeshipCheck(ServiceRequest serviceRequest = null)
        {
            var secondCheck = EmploymentCheck.New(Guid.NewGuid(), Id, EmploymentCheckType.EmployedAtStartOfApprenticeship, StartDate, StartDate.AddDays(42));
            Model.EmploymentCheckModels.Add(secondCheck.GetModel());

            AddEvent(new EmploymentChecksCreated(secondCheck.GetModel(), serviceRequest));
        }

        private void AddEmploymentBeforeSchemeCheck(ServiceRequest serviceRequest = null)
        {
            var phaseStartDate = GetPhaseStartDate();
            var firstCheck = EmploymentCheck.New(Guid.NewGuid(), Id, EmploymentCheckType.EmployedBeforeSchemeStarted, phaseStartDate.AddMonths(-6), phaseStartDate.AddDays(-1));
            Model.EmploymentCheckModels.Add(firstCheck.GetModel());

            AddEvent(new EmploymentChecksCreated(firstCheck.GetModel(), serviceRequest));
        }

        private void AddEmployedAt365PaymentDueDateFirstCheck(double checkWindowInDays = 21, ServiceRequest serviceRequest = null)
        {
            var secondPayment = Model.PendingPaymentModels.SingleOrDefault(pp => pp.EarningType == EarningType.SecondPayment && !pp.ClawedBack);
            if (secondPayment != null)
            {
                var initial365Check = EmploymentCheck.New(Guid.NewGuid(), Id, EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck, secondPayment.DueDate, secondPayment.DueDate.AddDays(checkWindowInDays));

                var existingCheck = Model.EmploymentCheckModels.SingleOrDefault(ec => ec.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck);

                if (existingCheck == null)
                {
                    Model.EmploymentCheckModels.Add(initial365Check.GetModel());

                    AddEvent(new EmploymentChecksCreated(initial365Check.GetModel(), serviceRequest));
                }
                else if (existingCheck.MinimumDate != initial365Check.MinimumDate ||
                       existingCheck.MaximumDate != initial365Check.MaximumDate)
                {
                    if (Model.EmploymentCheckModels.Remove(existingCheck))
                    {
                        AddEvent(new EmploymentCheckDeleted(existingCheck));
                    }

                    Model.EmploymentCheckModels.Add(initial365Check.GetModel());

                    AddEvent(new EmploymentChecksCreated(initial365Check.GetModel(), serviceRequest));
                }
            }
        }

        private void AddEmployedAt365PaymentDueDateSecondCheck(double checkWindowInDays = 42, ServiceRequest serviceRequest = null)
        {
            var secondPayment = Model.PendingPaymentModels.SingleOrDefault(pp => pp.EarningType == EarningType.SecondPayment && !pp.ClawedBack);

            if (secondPayment != null)
            {
                var second365Check = EmploymentCheck.New(Guid.NewGuid(), Id, EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck, secondPayment.DueDate, secondPayment.DueDate.AddDays(checkWindowInDays));

                var existingCheck = Model.EmploymentCheckModels.SingleOrDefault(ec => ec.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck);

                if (existingCheck == null)
                {
                    Model.EmploymentCheckModels.Add(second365Check.GetModel());

                    AddEvent(new EmploymentChecksCreated(second365Check.GetModel(), serviceRequest));
                }
                else if (existingCheck.MinimumDate != second365Check.MinimumDate ||
                       existingCheck.MaximumDate != second365Check.MaximumDate)
                {
                    if (Model.EmploymentCheckModels.Remove(existingCheck))
                    {
                        AddEvent(new EmploymentCheckDeleted(existingCheck));
                    }

                    Model.EmploymentCheckModels.Add(second365Check.GetModel());

                    AddEvent(new EmploymentChecksCreated(second365Check.GetModel(), serviceRequest));
                }
            }
        }

        public void RefreshLearner(Learner learner, IDateTimeService dateTimeService)
        {
            SetHasPossibleChangeOfCircumstances(learner.HasPossibleChangeOfCircumstances);
            LearnerRefreshCompleted();
            
            if (learner.SubmissionData.LearningData.LearningFound)
            {
                AddEmploymentChecks(dateTimeService);
            }
        }

        public void AddValidationOverride(ValidationOverrideStep validationOverrideStep, ServiceRequest serviceRequest)
        {
            RemoveValidationOverride(validationOverrideStep, serviceRequest);

            var validationOverride = ValidationOverride.New(Guid.NewGuid(), Model.Id, validationOverrideStep.ValidationType, validationOverrideStep.ExpiryDate);

            Model.ValidationOverrideModels.Add(validationOverride.GetModel());
            AddEvent(new ValidationOverrideCreated(validationOverride.Id, Model.Id, validationOverrideStep, serviceRequest));
        }

        public void RemoveValidationOverride(ValidationOverrideStep validationOverrideStep, ServiceRequest serviceRequest)
        {
            var existing = Model.ValidationOverrideModels.SingleOrDefault(x => x.Step == validationOverrideStep.ValidationType);

            if (existing != null)
            {
                Model.ValidationOverrideModels.Remove(existing);
                AddEvent(new ValidationOverrideDeleted(existing.Id, Model.Id, validationOverrideStep, serviceRequest));
            }
        }

        public void ExpireValidationOverrides(DateTime expireFrom)
        {
            Model.ValidationOverrideModels.ToList()
                .ForEach(vo =>
                {
                    if ((vo.ExpiryDate.Date <= expireFrom.Date) && Model.ValidationOverrideModels.Remove(vo))
                    {
                        AddEvent(new ValidationOverrideDeleted(vo.Id, Model.Id, new ValidationOverrideStep(vo.Step, vo.ExpiryDate), null));
                    }
                });
        }

        public void RevertPayment(Guid paymentId, ServiceRequest serviceRequest)
        {
            var payment = Model.PaymentModels.FirstOrDefault(x => x.Id == paymentId);
            if (payment == null)
            {
                return;
            }
            
            var pendingPayment = Model.PendingPaymentModels.FirstOrDefault(x => x.Id == payment.PendingPaymentId);
            if (pendingPayment == null)
            {
                return;
            }

            pendingPayment.PaymentMadeDate = null;

            Model.PaymentModels.Remove(payment);

            AddEvent(new PaymentReverted(payment, serviceRequest));
        }

        public void ReinstatePendingPayment(PendingPaymentModel pendingPaymentModel, ReinstatePaymentRequest reinstatePaymentRequest)
        {
            pendingPaymentModel.ClawedBack = false;
            pendingPaymentModel.PaymentMadeDate = null;
            pendingPaymentModel.CalculatedDate = DateTime.UtcNow;

            Model.PendingPaymentModels.Add(pendingPaymentModel);

            AddEvent(new PendingPaymentReinstated(pendingPaymentModel, reinstatePaymentRequest));
        }

        private DateTime GetPhaseStartDate()
        {
            if (Phase.Identifier == Enums.Phase.Phase1)
            {
                return Phase1Incentive.EligibilityStartDate;
            }
            if (Phase.Identifier == Enums.Phase.Phase2)
            {
                return Phase2Incentive.EligibilityStartDate;
            }
            if (Phase.Identifier == Enums.Phase.Phase3)
            {
                return Phase3Incentive.EligibilityStartDate;
            }

            throw new ArgumentException("Invalid phase!");
        }

        private ValidationOverrideStep GetOverrideStep(string validationStep)
        {
            var validationOverride = ValidationOverrides.SingleOrDefault(o => o.Step == validationStep);
            if (validationOverride == null)
            {
                return null;
            }
            return new ValidationOverrideStep(validationOverride.Step, validationOverride.ExpiryDate);
        }

        private void RemoveAll365Checks()
        {
            var existingChecks = Model.EmploymentCheckModels.Where(ec => ec.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck ||
                                                    ec.CheckType == EmploymentCheckType.EmployedAt365PaymentDueDateSecondCheck).ToList();

            foreach (var existingCheck in existingChecks)
            {
                if (Model.EmploymentCheckModels.Remove(existingCheck))
                {
                    AddEvent(new EmploymentCheckDeleted(existingCheck));
                }
            }
        }
        private bool HasSuccessfulChecks(IList<EmploymentCheckType> checkTypes)
        {
            var checks = new List<bool>();

            foreach (var checkType in checkTypes)
            {
                switch (checkType)
                {
                    case EmploymentCheckType.EmployedAtStartOfApprenticeship:

                        if (Model.EmploymentCheckModels.Any(c => c.CheckType == checkType && c.Result.HasValue && c.Result.Value))
                        {
                            checks.Add(true);
                        }
                        else
                        {
                            checks.Add(false);
                        }
                        break;

                    case EmploymentCheckType.EmployedBeforeSchemeStarted:

                        if (Model.EmploymentCheckModels.Any(c => c.CheckType == checkType && c.Result.HasValue && !c.Result.Value))
                        {
                            checks.Add(true);
                        }
                        else
                        {
                            checks.Add(false);
                        }
                        break;

                    case EmploymentCheckType.EmployedAt365PaymentDueDateFirstCheck:

                        if (Model.EmploymentCheckModels.Any(c => c.CheckType == checkType && c.Result.HasValue && c.Result.Value))
                        {
                            checks.Add(true);
                        }
                        else
                        {
                            checks.Add(false);
                        }
                        break;
                }
            }

            return checks.All(c => c);
        }

        private bool CanRefreshEmploymentCheck(EmploymentCheckType employmentCheckType)
        {
            var employmentCheck = Model.EmploymentCheckModels.FirstOrDefault(c => c.CheckType == employmentCheckType);
            if (employmentCheck == null)
            {
                return false;
            }

            if (employmentCheck.Result.HasValue)
            {
                return true;
            }
            
            if (!employmentCheck.Result.HasValue && employmentCheck.ErrorType != null)
            {
                return true;
            }

            return false;
        }
    }
}
