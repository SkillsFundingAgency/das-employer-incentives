using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
                Status = model.Status
            };
        }

        internal static ApprenticeshipIncentiveModel Map(this ApprenticeshipIncentive entity, IEnumerable<CollectionPeriod> collectionPeriods)
        {
            var apprenticeship = new Domain.ApprenticeshipIncentives.ValueTypes.Apprenticeship(
                     entity.ApprenticeshipId,
                     entity.FirstName,
                     entity.LastName,
                     entity.DateOfBirth,
                     entity.ULN,
                     entity.EmployerType,
                     entity.CourseName
                     );

            if (entity.UKPRN.HasValue)
            {
                apprenticeship.SetProvider(new Domain.ApprenticeshipIncentives.ValueTypes.Provider(entity.UKPRN.Value));
            }

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
                Status = entity.Status
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
                PeriodNumber = x.PeriodNumber,
                PaymentYear = x.PaymentYear,
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
                Result = x.Result,
                Step = x.Step,
                PendingPaymentId = paymentId
            }).ToList();
        }

        private static ICollection<PendingPaymentModel> Map(this ICollection<PendingPayment> models, IEnumerable<CollectionPeriod> collectionPeriods)
        {
            return models.Select(x => new PendingPaymentModel
            {
                Id = x.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(x.AccountId, x.AccountLegalEntityId),
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Amount = x.Amount,
                DueDate = x.DueDate,
                CalculatedDate = x.CalculatedDate,
                PeriodNumber = x.PeriodNumber,
                EarningType = x.EarningType,
                PaymentYear = x.PaymentYear,
                PaymentMadeDate = x.PaymentMadeDate,
                ClawedBack = x.ClawedBack,
                PendingPaymentValidationResultModels = x.ValidationResults.Map(collectionPeriods)
            }).ToList();
        }

        private static ICollection<PendingPaymentValidationResultModel> Map(this ICollection<PendingPaymentValidationResult> models, IEnumerable<CollectionPeriod> collectionPeriods)
        {
            return models.Select(x => new PendingPaymentValidationResultModel
            {
                Id = x.Id,
                CollectionPeriod = collectionPeriods.SingleOrDefault(p => Convert.ToInt16(p.AcademicYear) == x.PaymentYear && p.PeriodNumber == x.PeriodNumber).Map(),
                Result = x.Result,
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
                PendingPaymentId = x.PendingPaymentId
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
                PendingPaymentId = x.PendingPaymentId
            }).ToList();
        }

        private static Domain.ValueObjects.CollectionPeriod Map(this CollectionPeriod model)
        {
            if (model != null)
            {
                return new Domain.ValueObjects.CollectionPeriod(
                    model.PeriodNumber,
                    model.CalendarMonth,
                    model.CalendarYear,
                    model.EIScheduledOpenDateUTC,
                    model.CensusDate,
                    Convert.ToInt16(model.AcademicYear),
                    model.Active,
                    model.PeriodEndInProgress);
            }

            return null;
        }

        internal static ICollection<Domain.ValueObjects.CollectionPeriod> Map(this ICollection<CollectionPeriod> models)
        {
            return models.Select(x =>
                new Domain.ValueObjects.CollectionPeriod(
                    x.PeriodNumber,
                    x.CalendarMonth,
                    x.CalendarYear,
                    x.EIScheduledOpenDateUTC,
                    x.CensusDate,
                    Convert.ToInt16(x.AcademicYear),
                    x.Active,
                    x.PeriodEndInProgress)
            ).ToList();
        }

        internal static ICollection<CollectionPeriod> Map(this ICollection<Domain.ValueObjects.CollectionPeriod> models)
        {
            return models.Select(x =>
                new CollectionPeriod
                {
                    AcademicYear = x.AcademicYear.ToString(),
                    Active = x.Active,
                    CalendarMonth = x.CalendarMonth,
                    CalendarYear = x.CalendarYear,
                    CensusDate = x.CensusDate,
                    EIScheduledOpenDateUTC = x.OpenDate,
                    PeriodNumber = x.PeriodNumber,
                    PeriodEndInProgress = x.PeriodEndInProgress
                }).ToList();
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
                LearningPeriods = model.LearningPeriods.Map(),
                DaysInLearnings = model.DaysInLearnings.Map(),
                SubmissionData = new Domain.ApprenticeshipIncentives.ValueTypes.SubmissionData()
            };

            learner.SubmissionData.SetSubmissionDate(model.SubmissionDate);
            if (learner.SubmissionData.SubmissionFound)
            {
                learner.SubmissionData.SetLearningData(
                    new Domain.ApprenticeshipIncentives.ValueTypes.LearningData(model.LearningFound.Value));
                learner.SubmissionData.LearningData.SetStartDate(model.StartDate);

                if (model.HasDataLock.HasValue)
                {
                    learner.SubmissionData.LearningData.SetHasDataLock(model.HasDataLock.Value);
                }

                learner.SubmissionData.LearningData.SetIsInLearning(model.InLearning);

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
                DaysInLearnings = model.DaysInLearnings.Map(model.Id)
            };

            learner.SubmissionFound = model.SubmissionData.SubmissionFound;
            learner.LearningFound = model.SubmissionData.LearningData.LearningFound;
            learner.SubmissionDate = model.SubmissionData.SubmissionDate;
            learner.StartDate = model.SubmissionData.LearningData.StartDate;
            learner.HasDataLock = model.SubmissionData.LearningData.HasDataLock;
            learner.InLearning = model.SubmissionData.LearningData.IsInlearning;
            learner.RawJSON = model.SubmissionData.RawJson;

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

        private static ICollection<ApprenticeshipDaysInLearning> Map(this ICollection<Domain.ApprenticeshipIncentives.ValueTypes.DaysInLearning> models, Guid learnerId)
        {
            return models.Select(x => new ApprenticeshipDaysInLearning
            {
                LearnerId = learnerId,
                CollectionPeriodNumber = x.CollectionPeriodNumber,
                CollectionPeriodYear = x.CollectionYear,
                NumberOfDaysInLearning = x.NumberOfDays
            }).ToList();
        }

        private static ICollection<Domain.ApprenticeshipIncentives.ValueTypes.LearningPeriod> Map(this ICollection<LearningPeriod> models)
        {
            return models.Select(x => new Domain.ApprenticeshipIncentives.ValueTypes.LearningPeriod(x.StartDate, x.EndDate)).ToList();
        }

        private static ICollection<Domain.ApprenticeshipIncentives.ValueTypes.DaysInLearning> Map(this ICollection<ApprenticeshipDaysInLearning> models)
        {
            return models.Select(x => new Domain.ApprenticeshipIncentives.ValueTypes.DaysInLearning(x.CollectionPeriodNumber, x.CollectionPeriodYear, x.NumberOfDaysInLearning)).ToList();
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
                CollectionPeriod = x.CollectionPeriod,
                CollectionPeriodYear = x.CollectionPeriodYear
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
                CollectionPeriod = x.CollectionPeriod,
                CollectionPeriodYear = x.CollectionPeriodYear
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
    }
}
