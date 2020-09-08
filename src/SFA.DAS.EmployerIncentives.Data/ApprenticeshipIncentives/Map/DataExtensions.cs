using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
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
                PendingPayments = model.PendingPaymentModels.Map()
            };
        }

        internal static ApprenticeshipIncentiveModel Map(this ApprenticeshipIncentive entity)
        {
            return new ApprenticeshipIncentiveModel
            {
                 Id = entity.Id,
                 Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(entity.AccountId),
                 Apprenticeship = new Domain.ApprenticeshipIncentives.ValueTypes.Apprenticeship(
                     entity.ApprenticeshipId,
                     entity.FirstName,
                     entity.LastName,
                     entity.DateOfBirth,
                     entity.Uln,
                     entity.EmployerType
                     ),
                 PendingPaymentModels = entity.PendingPayments.Map()
            };
        }

        private static ICollection<PendingPayment> Map(this ICollection<PendingPaymentModel> models)
        {
            return models.Select(x => new PendingPayment
            {                
                Id = x.Id,
                AccountId = x.Account.Id,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                AmountPayablePence = x.AmountInPence,
                DatePayable = x.DatePayable,
                DateCalculated = x.DateCalculated
            }).ToList();
        }

        private static ICollection<PendingPaymentModel> Map(this ICollection<PendingPayment> models)
        {
            return models.Select(x => new PendingPaymentModel
            {
                Id = x.Id,
                Account = new Domain.ApprenticeshipIncentives.ValueTypes.Account(x.AccountId),
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId,
                AmountInPence = x.AmountPayablePence,
                DatePayable = x.DatePayable,
                DateCalculated = x.DateCalculated
            }).ToList();
        }
    }
}
