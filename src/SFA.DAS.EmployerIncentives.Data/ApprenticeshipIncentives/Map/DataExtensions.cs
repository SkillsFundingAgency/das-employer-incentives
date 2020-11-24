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
                PlannedStartDate = model.PlannedStartDate,
                IncentiveApplicationApprenticeshipId = model.ApplicationApprenticeshipId,
                PendingPayments = model.PendingPaymentModels.Map(),
                Payments = model.PaymentModels.Map(),
                AccountLegalEntityId = model.Account.AccountLegalEntityId,
                ActualStartDate = model.ActualStartDate,
                RefreshedLearnerForEarnings = model.RefreshedLearnerForEarnings,
                HasPossibleChangeOfCircumstances = model.HasPossibleChangeOfCircumstances
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
                     entity.EmployerType
                     );

            if (entity.UKPRN.HasValue)
            {
                apprenticeship.SetProvider(new Domain.ApprenticeshipIncentives.ValueTypes.Provider(entity.UKPRN.Value));
            }

            return new ApprenticeshipIncentiveModel
            {
                Id = entity.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(entity.AccountId, entity.AccountLegalEntityId.HasValue ? entity.AccountLegalEntityId.Value : 0),
                Apprenticeship = apprenticeship,
                PlannedStartDate = entity.PlannedStartDate,
                ApplicationApprenticeshipId = entity.IncentiveApplicationApprenticeshipId,
                PendingPaymentModels = entity.PendingPayments.Map(collectionPeriods),
                PaymentModels = entity.Payments.Map(),
                ActualStartDate = entity.ActualStartDate,
                RefreshedLearnerForEarnings = entity.RefreshedLearnerForEarnings,
                HasPossibleChangeOfCircumstances = entity.HasPossibleChangeOfCircumstances
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
                ValidationResults = x.PendingPaymentValidationResultModels.Map(x.Id),
            }).ToList();
        }

        private static ICollection<PendingPaymentValidationResult> Map(
            this ICollection<PendingPaymentValidationResultModel> models, Guid paymentId)
        {
            return models.Select(x => new PendingPaymentValidationResult
            {
                Id = x.Id,
                CollectionDateUtc = x.CollectionPeriod.OpenDate,
                CollectionPeriodMonth = x.CollectionPeriod.CalendarMonth,
                CollectionPeriodYear = x.CollectionPeriod.CalendarYear,
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
                PendingPaymentValidationResultModels = x.ValidationResults.Map(collectionPeriods)
            }).ToList();
        }

        private static ICollection<PendingPaymentValidationResultModel> Map(this ICollection<PendingPaymentValidationResult> models, IEnumerable<CollectionPeriod> collectionPeriods)
        {
            return models.Select(x => new PendingPaymentValidationResultModel
            {
                Id = x.Id,
                CollectionPeriod = collectionPeriods.SingleOrDefault(p => p.CalendarYear == x.CollectionPeriodYear && p.CalendarMonth == x.CollectionPeriodMonth).Map(),
                Result = x.Result,
                DateTime = x.CollectionDateUtc,
                Step = x.Step
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
                return new Domain.ValueObjects.CollectionPeriod(model.PeriodNumber, model.CalendarMonth, model.CalendarYear, model.EIScheduledOpenDateUTC,
                    DateTime.Now, DateTime.Now.Year.ToString(), true);
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
                    x.AcademicYear,
                    x.Active)
            ).ToList();
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
            };

            if (model.SubmissionFound)
            {
                learner.SubmissionData = new Domain.ApprenticeshipIncentives.ValueTypes.SubmissionData(model.SubmissionDate.Value);

                if (model.LearningFound.HasValue)
                {
                    learner.SubmissionData.SetLearningFound(new Domain.ApprenticeshipIncentives.ValueTypes.LearningFoundStatus(model.LearningFound.Value));
                }
                if (model.HasDataLock.HasValue)
                {
                    learner.SubmissionData.SetHasDataLock(model.HasDataLock.Value);
                }
                learner.SubmissionData.SetIsInLearning(model.InLearning);
                learner.SubmissionData.SetRawJson(model.RawJSON);
                learner.SubmissionData.SetStartDate(model.StartDate);
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
                CreatedDate = model.CreatedDate
            };

            if (model.SubmissionData != null)
            {
                learner.SubmissionFound = true;
                learner.LearningFound = model.SubmissionData.LearningFoundStatus?.LearningFound;
                learner.SubmissionDate = model.SubmissionData.SubmissionDate;
                learner.StartDate = model.SubmissionData.StartDate;
                learner.HasDataLock = model.SubmissionData.HasDataLock;
                learner.InLearning = model.SubmissionData.IsInlearning;
                learner.RawJSON = model.SubmissionData.RawJson;
            }

            return learner;
        }
    }
}
