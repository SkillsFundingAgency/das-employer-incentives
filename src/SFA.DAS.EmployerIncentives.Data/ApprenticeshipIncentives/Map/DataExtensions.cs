﻿using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
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

        internal static ApprenticeshipIncentiveModel Map(this ApprenticeshipIncentive entity)
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
                 PendingPaymentModels = entity.PendingPayments.Map()
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
                EarningType = x.EarningType
            }).ToList();
        }

        private static ICollection<PendingPaymentModel> Map(this ICollection<PendingPayment> models)
        {
            return models.Select(x => new PendingPaymentModel
            {
                Id = x.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(x.AccountId, x.AccountLegalEntityId),
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                Amount = x.Amount,
                DueDate = x.DueDate,
                CalculatedDate = x.CalculatedDate,
                EarningType = x.EarningType
            }).ToList();
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
      
    }
}
