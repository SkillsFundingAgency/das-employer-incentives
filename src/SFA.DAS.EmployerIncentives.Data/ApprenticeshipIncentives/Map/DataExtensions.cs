﻿using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using ChangeOfCircumstance = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.ChangeOfCircumstance;
using LearningPeriod = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.LearningPeriod;
using PendingPaymentValidationResult = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.PendingPaymentValidationResult;
using ReinstatedPendingPaymentAudit = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.ReinstatedPendingPaymentAudit;
using RevertedPaymentAudit = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.RevertedPaymentAudit;
using VendorBlockAudit = SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models.VendorBlockAudit;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map
{
    public static class DataExtensions
    {
        internal static ApprenticeshipIncentive Map(this ApprenticeshipIncentiveModel model)
        {
            return new ApprenticeshipIncentive
            {
                Id = model.Id,
                AccountId = model.Account.Id,
                ApprenticeshipId = model.Apprenticeship.Id,
                FirstName = model.Apprenticeship.FirstName,
                LastName = model.Apprenticeship.LastName,
                DateOfBirth = model.Apprenticeship.DateOfBirth,
                ULN = model.Apprenticeship.UniqueLearnerNumber,
                UKPRN = model.Apprenticeship.Provider?.Ukprn,
                EmployerType = model.Apprenticeship.EmployerType,
                StartDate = model.StartDate,
                IncentiveApplicationApprenticeshipId = model.ApplicationApprenticeshipId,
                PendingPayments = model.PendingPaymentModels.Map(),
                Payments = model.PaymentModels.Map(),
                ClawbackPayments = model.ClawbackPaymentModels.Map(),
                RefreshedLearnerForEarnings = model.RefreshedLearnerForEarnings,
                HasPossibleChangeOfCircumstances = model.HasPossibleChangeOfCircumstances,
                AccountLegalEntityId = model.Account.AccountLegalEntityId,
                PausePayments = model.PausePayments,
                SubmittedDate = model.SubmittedDate,
                SubmittedByEmail = model.SubmittedByEmail,
                CourseName = model.Apprenticeship.CourseName,
                EmploymentStartDate = model.Apprenticeship.EmploymentStartDate,
                Status = model.Status,
                BreakInLearnings = model.BreakInLearnings.Map(model.Id),
                MinimumAgreementVersion = model.MinimumAgreementVersion.MinimumRequiredVersion,
                Phase = model.Phase.Identifier,
                WithdrawnBy = model.WithdrawnBy,
                EmploymentChecks = model.EmploymentCheckModels.Map(),
                ValidationOverrides = model.ValidationOverrideModels.Map()
            };
        }

        internal static ApprenticeshipIncentiveModel Map(this ApprenticeshipIncentive entity, IEnumerable<CollectionCalendarPeriod> collectionPeriods)
        {
            Provider provider = null;
            if (entity.UKPRN.HasValue)
            {
                provider = new Provider(entity.UKPRN.Value);
            }

            var apprenticeship = new Apprenticeship(
                     entity.ApprenticeshipId,
                     entity.FirstName,
                     entity.LastName,
                     entity.DateOfBirth,
                     entity.ULN,
                     entity.EmployerType,
                     entity.CourseName,
                     entity.EmploymentStartDate,
                     provider
                     );
            
            return new ApprenticeshipIncentiveModel
            {
                Id = entity.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(entity.AccountId, entity.AccountLegalEntityId),
                Apprenticeship = apprenticeship,
                StartDate = entity.StartDate,
                ApplicationApprenticeshipId = entity.IncentiveApplicationApprenticeshipId,
                PendingPaymentModels = entity.PendingPayments.Map(collectionPeriods),
                PaymentModels = entity.Payments.Map(),
                ClawbackPaymentModels = entity.ClawbackPayments.Map(),
                RefreshedLearnerForEarnings = entity.RefreshedLearnerForEarnings,
                HasPossibleChangeOfCircumstances = entity.HasPossibleChangeOfCircumstances,
                PausePayments = entity.PausePayments,
                SubmittedDate = entity.SubmittedDate,
                SubmittedByEmail = entity.SubmittedByEmail,
                Status = entity.Status,
                PreviousStatus = entity.Status,
                BreakInLearnings = entity.BreakInLearnings.Map(),                
                MinimumAgreementVersion = entity.MinimumAgreementVersion.HasValue ? new AgreementVersion(entity.MinimumAgreementVersion.Value) : AgreementVersion.Create(entity.Phase, entity.StartDate),
                Phase = new Domain.ValueObjects.IncentivePhase(entity.Phase),
                WithdrawnBy = entity.WithdrawnBy,
                EmploymentCheckModels = entity.EmploymentChecks.Map(),
                ValidationOverrideModels = entity.ValidationOverrides.Map(),                
            };
        }

        private static ICollection<PendingPayment> Map(this ICollection<PendingPaymentModel> models)
        {
            return models.Select(x => new PendingPayment
            {
                Id = x.Id,
                AccountId = x.Account.Id,
                AccountLegalEntityId = x.Account.AccountLegalEntityId,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Amount = x.Amount,
                DueDate = x.DueDate,
                CalculatedDate = x.CalculatedDate,
                PeriodNumber = x.CollectionPeriod?.PeriodNumber,
                PaymentYear = x.CollectionPeriod.AcademicYear,
                PaymentMadeDate = x.PaymentMadeDate,
                EarningType = x.EarningType,
                ClawedBack = x.ClawedBack,
                ValidationResults = x.PendingPaymentValidationResultModels.Map(x.Id),
            }).ToList();
        }

        private static ICollection<PendingPaymentValidationResult> Map(
            this ICollection<PendingPaymentValidationResultModel> models, Guid paymentId)
        {
            return models.Select(x => new PendingPaymentValidationResult
            {
                Id = x.Id,
                CreatedDateUtc = x.CreatedDateUtc,
                PeriodNumber = x.CollectionPeriod.PeriodNumber,
                PaymentYear = x.CollectionPeriod.AcademicYear,
                Result = x.ValidationResult,
                OverrideResult = x.OverrideResult,
                Step = x.Step,
                PendingPaymentId = paymentId
            }).ToList();
        }

        private static ICollection<PendingPaymentModel> Map(this ICollection<PendingPayment> models, IEnumerable<CollectionCalendarPeriod> collectionPeriods)
        {
            return models.Select(x => new PendingPaymentModel
            {
                Id = x.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(x.AccountId, x.AccountLegalEntityId),
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Amount = x.Amount,
                DueDate = x.DueDate,
                CalculatedDate = x.CalculatedDate,
                CollectionPeriod = x.PeriodNumber.HasValue && x.PaymentYear.HasValue ? new Domain.ValueObjects.CollectionPeriod(x.PeriodNumber.Value, x.PaymentYear.Value) : null,
                EarningType = x.EarningType,
                PaymentMadeDate = x.PaymentMadeDate,
                ClawedBack = x.ClawedBack,
                PendingPaymentValidationResultModels = x.ValidationResults.Map(collectionPeriods)
            }).ToList();
        }

        private static ICollection<PendingPaymentValidationResultModel> Map(this ICollection<PendingPaymentValidationResult> models, IEnumerable<CollectionCalendarPeriod> collectionPeriods)
        {
            return models.Select(x => new PendingPaymentValidationResultModel
            {
                Id = x.Id,
                CollectionPeriod = collectionPeriods.SingleOrDefault(p => Convert.ToInt16(p.AcademicYear) == x.PaymentYear && p.PeriodNumber == x.PeriodNumber).Map(),
                ValidationResult = x.Result,
                OverrideResult = x.OverrideResult ?? false,
                Step = x.Step,
                CreatedDateUtc = x.CreatedDateUtc
            }).ToList();
        }
        private static ICollection<Payment> Map(this ICollection<PaymentModel> models)
        {
            return models.Select(x => new Payment
            {
                Id = x.Id,
                AccountId = x.Account.Id,
                AccountLegalEntityId = x.Account.AccountLegalEntityId,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Amount = x.Amount,
                CalculatedDate = x.CalculatedDate,
                PaymentPeriod = x.PaymentPeriod,
                PaymentYear = x.PaymentYear,
                PaidDate = x.PaidDate,
                SubnominalCode = x.SubnominalCode,
                PendingPaymentId = x.PendingPaymentId,
                VrfVendorId = x.VrfVendorId
            }).ToList();
        }

        private static ICollection<PaymentModel> Map(this ICollection<Payment> models)
        {
            return models.Select(x => new PaymentModel
            {
                Id = x.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(x.AccountId, x.AccountLegalEntityId),
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Amount = x.Amount,
                CalculatedDate = x.CalculatedDate,
                PaidDate = x.PaidDate,
                PaymentPeriod = x.PaymentPeriod,
                PaymentYear = x.PaymentYear,
                SubnominalCode = x.SubnominalCode,
                PendingPaymentId = x.PendingPaymentId,
                VrfVendorId = x.VrfVendorId
            }).ToList();
        }
      
        private static Domain.ValueObjects.CollectionPeriod Map(this CollectionCalendarPeriod model)
        {
            if (model != null)
            {
                var period = new Domain.ValueObjects.CollectionPeriod(
                    model.PeriodNumber,
                    Convert.ToInt16(model.AcademicYear));

                return period;
            }

            return null;
        }

        internal static ICollection<Domain.ValueObjects.CollectionCalendarPeriod> Map(this ICollection<CollectionCalendarPeriod> models)
        {
            return models.Select(x => x.MapCollectionCalendarPeriod()).ToList();
        }        

        internal static ICollection<CollectionCalendarPeriod> Map(this ICollection<Domain.ValueObjects.CollectionCalendarPeriod> models)
        {
            return models.Select(x =>
                new CollectionCalendarPeriod
                {
                    AcademicYear = x.CollectionPeriod.AcademicYear.ToString(),
                    Active = x.Active,
                    CalendarMonth = x.CalendarMonth,
                    CalendarYear = x.CalendarYear,
                    CensusDate = x.CensusDate,
                    EIScheduledOpenDateUTC = x.OpenDate,
                    PeriodNumber = x.CollectionPeriod.PeriodNumber,
                    PeriodEndInProgress = x.PeriodEndInProgress,
                    MonthEndProcessingCompleteUTC = x.MonthEndProcessingCompletedDate
                }).ToList();
        }

        private static Domain.ValueObjects.AcademicYear Map(this AcademicYear model)
        {
            if (model != null)
            {
                var academicYear = new Domain.ValueObjects.AcademicYear(
                    model.Id,
                    model.EndDate);

                return academicYear;
            }

            return null;
        }

        internal static ICollection<Domain.ValueObjects.AcademicYear> Map(this ICollection<AcademicYear> models)
        {
            return models.Select(x => x.Map()).ToList();
        }

        internal static LearnerModel Map(this Learner model)
        {
            var learner = new LearnerModel
            {
                Id = model.Id,
                ApprenticeshipIncentiveId = model.ApprenticeshipIncentiveId,
                ApprenticeshipId = model.ApprenticeshipId,
                Ukprn = model.Ukprn,
                UniqueLearnerNumber = model.ULN,
                CreatedDate = model.CreatedDate,
                UpdatedDate = model.UpdatedDate,
                RefreshDate = model.RefreshDate,
                SuccessfulLearnerMatch = model.SuccessfulLearnerMatchExecution,
                LearningPeriods = model.LearningPeriods.Map(),
                DaysInLearnings = model.DaysInLearnings.Map(),
                SubmissionData = new SubmissionData()
            };

            learner.SubmissionData.SetSubmissionDate(model.SubmissionDate);
            if (learner.SubmissionData.SubmissionFound)
            {
                learner.SubmissionData.SetLearningData(
                    new LearningData(model.LearningFound.Value));
                learner.SubmissionData.LearningData.SetStartDate(model.StartDate);

                if (model.HasDataLock.HasValue)
                {
                    learner.SubmissionData.LearningData.SetHasDataLock(model.HasDataLock.Value);
                }

                learner.SubmissionData.LearningData.SetIsInLearning(model.InLearning);

                if(model.LearningStoppedDate.HasValue)
                {
                    learner.SubmissionData.LearningData.SetIsStopped(new LearningStoppedStatus(true, model.LearningStoppedDate.Value));
                }

                if (model.LearningResumedDate.HasValue)
                {
                    learner.SubmissionData.LearningData.SetIsStopped(new LearningStoppedStatus(false, model.LearningResumedDate.Value));
                }

                learner.SubmissionData.SetRawJson(model.RawJSON);
            }

            return learner;
        }

        internal static Learner Map(this LearnerModel model)
        {
            var learner = new Learner
            {
                Id = model.Id,
                ApprenticeshipIncentiveId = model.ApprenticeshipIncentiveId,
                ApprenticeshipId = model.ApprenticeshipId,
                Ukprn = model.Ukprn,
                ULN = model.UniqueLearnerNumber,
                LearningPeriods = model.LearningPeriods.Map(model.Id),
                DaysInLearnings = model.DaysInLearnings.Map(model.Id),
                SubmissionFound = model.SubmissionData.SubmissionFound,
                LearningFound = model.SubmissionData.LearningData.LearningFound,
                SubmissionDate = model.SubmissionData.SubmissionDate,
                StartDate = model.SubmissionData.LearningData.StartDate,
                HasDataLock = model.SubmissionData.LearningData.HasDataLock,
                InLearning = model.SubmissionData.LearningData.IsInlearning,
                RawJSON = model.SubmissionData.RawJson,
                LearningStoppedDate = model.SubmissionData.LearningData?.StoppedStatus?.DateStopped,
                LearningResumedDate = model.SubmissionData.LearningData?.StoppedStatus?.DateResumed,
                SuccessfulLearnerMatchExecution = model.SuccessfulLearnerMatch,
                RefreshDate = model.RefreshDate
            };

            return learner;
        }

        private static ICollection<LearningPeriod> Map(this ICollection<Domain.ApprenticeshipIncentives.ValueTypes.LearningPeriod> models, Guid learnerId)
        {
            return models.Select(x => new LearningPeriod
            {
                LearnerId = learnerId,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            }).ToList();
        }

        private static ICollection<ApprenticeshipDaysInLearning> Map(this ICollection<DaysInLearning> models, Guid learnerId)
        {
            return models.Select(x => new ApprenticeshipDaysInLearning
            {
                LearnerId = learnerId,
                CollectionPeriodNumber = x.CollectionPeriod.PeriodNumber,
                CollectionPeriodYear = x.CollectionPeriod.AcademicYear,
                NumberOfDaysInLearning = x.NumberOfDays
            }).ToList();
        }

        private static ICollection<Domain.ApprenticeshipIncentives.ValueTypes.LearningPeriod> Map(this ICollection<LearningPeriod> models)
        {
            return models.Select(x => new Domain.ApprenticeshipIncentives.ValueTypes.LearningPeriod(x.StartDate, x.EndDate)).ToList();
        }

        private static ICollection<DaysInLearning> Map(this ICollection<ApprenticeshipDaysInLearning> models)
        {
            return models.Select(x => new DaysInLearning(new Domain.ValueObjects.CollectionPeriod(x.CollectionPeriodNumber, x.CollectionPeriodYear), x.NumberOfDaysInLearning)).ToList();
        }

        private static ICollection<BreakInLearning> Map(this ICollection<ApprenticeshipBreakInLearning> models)
        {
            return models.Select(x => x.Map()).ToList();
        }

        private static BreakInLearning Map(this ApprenticeshipBreakInLearning model)
        {
            var newBreakInLearning = new BreakInLearning(model.StartDate);
            if (model.EndDate.HasValue)
            {
                newBreakInLearning = BreakInLearning.Create(model.StartDate, model.EndDate.Value);
            }

            return newBreakInLearning;
        }

        private static ICollection<ApprenticeshipBreakInLearning> Map(this ICollection<BreakInLearning> models, Guid ApprenticeshipIncentiveId)
        {
            return models.Select(x => new ApprenticeshipBreakInLearning
            {
                ApprenticeshipIncentiveId = ApprenticeshipIncentiveId,
                StartDate = x.StartDate,
                EndDate = x.EndDate                
            }).ToList();
        }

        private static ICollection<ClawbackPayment> Map(this ICollection<ClawbackPaymentModel> models)
        {
            return models.Select(x => new ClawbackPayment
            {
                Id = x.Id,
                AccountId = x.Account.Id,
                AccountLegalEntityId = x.Account.AccountLegalEntityId,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                PendingPaymentId = x.PendingPaymentId,
                Amount = x.Amount,
                DateClawbackCreated = x.CreatedDate,
                PaymentId = x.PaymentId,
                SubnominalCode = x.SubnominalCode,
                DateClawbackSent = x.DateClawbackSent,
                CollectionPeriod = x.CollectionPeriod.PeriodNumber,
                CollectionPeriodYear = x.CollectionPeriod.AcademicYear,
                VrfVendorId = x.VrfVendorId
            }).ToList();
        }

        private static ICollection<ClawbackPaymentModel> Map(this ICollection<ClawbackPayment> models)
        {
            return models.Select(x => new ClawbackPaymentModel
            {
                Id = x.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(x.AccountId, x.AccountLegalEntityId),
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Amount = x.Amount,
                CreatedDate = x.DateClawbackCreated,
                PaymentId = x.PaymentId,
                SubnominalCode = x.SubnominalCode,
                PendingPaymentId = x.PendingPaymentId,
                DateClawbackSent = x.DateClawbackSent,
                CollectionPeriod = x.CollectionPeriod.HasValue ? new Domain.ValueObjects.CollectionPeriod(x.CollectionPeriod.Value, x.CollectionPeriodYear.Value) : null,
                VrfVendorId = x.VrfVendorId
            }).ToList();
        }

        internal static ChangeOfCircumstance Map(this Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance model)
        {
            return new ChangeOfCircumstance
            {
                Id = model.Id,
                ApprenticeshipIncentiveId = model.ApprenticeshipIncentiveId,
                ChangeType = model.Type,
                PreviousValue = model.PreviousValue,
                NewValue = model.NewValue,
                ChangedDate = model.ChangedDate
            };
        }

        internal static Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance Map(this ChangeOfCircumstance model)
        {
            return new Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance(
                model.Id,
                model.ApprenticeshipIncentiveId,
                model.ChangeType,
                model.PreviousValue,
                model.NewValue,
                model.ChangedDate);
        }

        internal static List<Domain.ApprenticeshipIncentives.ValueTypes.ChangeOfCircumstance> Map(this List<ChangeOfCircumstance> models)
        {
            return models.Select(model => model.Map()).ToList();
        }

        private static ICollection<EmploymentCheck> Map(this ICollection<EmploymentCheckModel> models)
        {
            return models.Select(x => new EmploymentCheck
            {
                Id = x.Id,
                MaximumDate = x.MaximumDate,
                MinimumDate = x.MinimumDate,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                CheckType = x.CheckType,
                CorrelationId = x.CorrelationId,
                CreatedDateTime = x.CreatedDateTime,
                UpdatedDateTime = x.UpdatedDateTime,
                Result = x.Result,
                ErrorType = x.ErrorType,
                ResultDateTime = x.ResultDateTime
            }).ToList();
        }

        private static ICollection<EmploymentCheckModel> Map(this ICollection<EmploymentCheck> models)
        {
            return models.Select(x => new EmploymentCheckModel
            {
                Id = x.Id,
                MaximumDate = x.MaximumDate,
                MinimumDate = x.MinimumDate,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                CheckType = x.CheckType,
                CorrelationId = x.CorrelationId,
                CreatedDateTime = x.CreatedDateTime,
                UpdatedDateTime = x.UpdatedDateTime,
                Result = x.Result,
                ErrorType = x.ErrorType,
                ResultDateTime = x.ResultDateTime
            }).ToList();
        }

        private static ICollection<ValidationOverrideModel> Map(this ICollection<ValidationOverride> models)
        {
            return models.Select(x => new ValidationOverrideModel
            {
                Id = x.Id,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Step = x.Step,
                ExpiryDate = x.ExpiryDate,
                CreatedDate = x.CreatedDateTime
            }).ToList();
        }

        private static ICollection<ValidationOverride> Map(this ICollection<ValidationOverrideModel> models)
        {
            return models.Select(x => new ValidationOverride
            {
                Id = x.Id,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Step = x.Step,
                ExpiryDate = x.ExpiryDate,
                CreatedDateTime = x.CreatedDate
            }).ToList();
        }     

        internal static EmploymentCheckAudit Map(this EmploymentCheckRequestAudit entity)
        {
            return new EmploymentCheckAudit
            {
                Id = entity.Id,
                ApprenticeshipIncentiveId = entity.ApprenticeshipIncentiveId,
                CheckType = entity.CheckType.ToString(),
                ServiceRequestTaskId = entity.ServiceRequest.TaskId,
                ServiceRequestDecisionReference = entity.ServiceRequest.DecisionReference,
                ServiceRequestCreatedDate = entity.ServiceRequest.Created,
                CreatedDateTime = DateTime.UtcNow
            };
        }

        internal static ValidationOverrideAudit Map(this ValidationOverrideStepAudit entity)
        {
            return new ValidationOverrideAudit
            {
                Id = entity.Id,
                ApprenticeshipIncentiveId = entity.ApprenticeshipIncentiveId,
                Step = entity.ValidationOverrideStep.ValidationType,
                ExpiryDate = entity.ValidationOverrideStep.ExpiryDate,
                ServiceRequestTaskId = entity.ServiceRequest.TaskId,
                ServiceRequestDecisionReference = entity.ServiceRequest.DecisionReference,
                ServiceRequestCreatedDate = entity.ServiceRequest.Created,
                CreatedDateTime = DateTime.UtcNow                
            };
        }

        internal static RevertedPaymentAudit Map(this Domain.ApprenticeshipIncentives.ValueTypes.RevertedPaymentAudit model)
        {
            return new RevertedPaymentAudit
            {
                Id = model.Id,
                ApprenticeshipIncentiveId = model.ApprenticeshipIncentiveId,
                PaymentId = model.PaymentId,
                PendingPaymentId = model.PendingPaymentId,
                PaymentPeriod = model.PaymentPeriod,
                PaymentYear = model.PaymentYear,
                Amount = model.Amount,
                CalculatedDate = model.CalculatedDate,
                PaidDate = model.PaidDate,
                VrfVendorId = model.VrfVendorId,
                ServiceRequestTaskId = model.ServiceRequest.TaskId,
                ServiceRequestDecisionReference = model.ServiceRequest.DecisionReference,
                ServiceRequestCreatedDate = model.ServiceRequest.Created,
                CreatedDateTime = model.CreatedDateTime
            };
        }

        internal static ReinstatedPendingPaymentAudit Map(this Domain.ApprenticeshipIncentives.ValueTypes.ReinstatedPendingPaymentAudit model)
        {
            return new ReinstatedPendingPaymentAudit
            {
                Id = model.Id,
                ApprenticeshipIncentiveId = model.ApprenticeshipIncentiveId,
                PendingPaymentId = model.PendingPaymentId,
                ServiceRequestTaskId = model.ReinstatePaymentRequest.TaskId,
                ServiceRequestDecisionReference = model.ReinstatePaymentRequest.DecisionReference,
                ServiceRequestCreatedDate = model.ReinstatePaymentRequest.Created,
                Process = model.ReinstatePaymentRequest.Process,
                CreatedDateTime = model.CreatedDateTime
            };
        }

        internal static VendorBlockAudit Map(this VendorBlockRequestAudit entity)
        {
            return new VendorBlockAudit
            {
                Id = entity.Id,
                VrfVendorId = entity.VrfVendorId,
                ServiceRequestTaskId = entity.ServiceRequest.TaskId,
                ServiceRequestDecisionReference = entity.ServiceRequest.DecisionReference,
                ServiceRequestCreatedDate = entity.ServiceRequest.Created,
                VendorBlockEndDate = entity.VendorBlockEndDate,
                CreatedDateTime = DateTime.UtcNow
            };
        }

        internal static Domain.ValueObjects.CollectionCalendarPeriod MapCollectionCalendarPeriod(this CollectionCalendarPeriod model)
        {
            if (model != null)
            {
                var collectionCalendarPeriod = new Domain.ValueObjects.CollectionCalendarPeriod(
                    new Domain.ValueObjects.CollectionPeriod(model.PeriodNumber, Convert.ToInt16(model.AcademicYear)),
                    model.CalendarMonth,
                    model.CalendarYear,
                    model.EIScheduledOpenDateUTC,
                    model.CensusDate,
                    model.Active,
                    model.PeriodEndInProgress);

                if (model.MonthEndProcessingCompleteUTC.HasValue)
                {
                    collectionCalendarPeriod.SetMonthEndProcessingCompletedDate(model.MonthEndProcessingCompleteUTC.Value);
                }

                return collectionCalendarPeriod;

            }

            return null;
        }

    }
}
