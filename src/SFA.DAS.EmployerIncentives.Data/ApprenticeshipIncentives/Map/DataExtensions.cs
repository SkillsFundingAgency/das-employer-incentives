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
                Uln = model.Apprenticeship.UniqueLearnerNumber,
                EmployerType = model.Apprenticeship.EmployerType,
                PlannedStartDate = model.PlannedStartDate,
                IncentiveApplicationApprenticeshipId = model.ApplicationApprenticeshipId,
                PendingPayments = model.PendingPaymentModels.Map(),
                AccountLegalEntityId = model.Account.AccountLegalEntityId
            };
        }

        internal static ApprenticeshipIncentiveModel Map(this ApprenticeshipIncentive entity, IEnumerable<CollectionPeriod> collectionPeriods)
        {            
            return new ApprenticeshipIncentiveModel
            {
                Id = entity.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(entity.AccountId, entity.AccountLegalEntityId.HasValue ? entity.AccountLegalEntityId.Value : 0),
                Apprenticeship = new Domain.ApprenticeshipIncentives.ValueTypes.Apprenticeship(
                     entity.ApprenticeshipId,
                     entity.FirstName,
                     entity.LastName,
                     entity.DateOfBirth,
                     entity.Uln,
                     entity.EmployerType
                     ),
                PlannedStartDate = entity.PlannedStartDate,
                ApplicationApprenticeshipId = entity.IncentiveApplicationApprenticeshipId,
                PendingPaymentModels = entity.PendingPayments.Map(collectionPeriods)
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
                 CollectionPeriod =  collectionPeriods.SingleOrDefault(p => p.CalendarYear == x.CollectionPeriodYear && p.CalendarMonth == x.CollectionPeriodMonth).Map(),
                 Result = x.Result,
                 DateTime = x.CollectionDateUtc,
                 Step = x.Step
            }).ToList();
        }

        private static Domain.ValueObjects.CollectionPeriod Map(this CollectionPeriod model)
        {
            if (model != null)
            {
                return new Domain.ValueObjects.CollectionPeriod(model.PeriodNumber, model.CalendarMonth, model.CalendarYear, model.EIScheduledOpenDateUTC);
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
                    x.EIScheduledOpenDateUTC)
            ).ToList();
        }

    }
}
