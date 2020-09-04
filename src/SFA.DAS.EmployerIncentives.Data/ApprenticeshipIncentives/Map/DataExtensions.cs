using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map
{
    public static class DataExtensions
    {
        internal static Models.ApprenticeshipIncentive Map(this ApprenticeshipIncentiveModel model)
        {
            return new Models.ApprenticeshipIncentive
            {
                Id = model.Id,
                AccountId = model.Account.Id,
                ApprenticeshipId = model.Apprenticeship.Id,
                FirstName = model.Apprenticeship.FirstName,
                LastName = model.Apprenticeship.LastName,
                DateOfBirth = model.Apprenticeship.DateOfBirth,
                Uln = model.Apprenticeship.UniqueLearnerNumber,
                EmployerType = model.Apprenticeship.EmployerType
            };
        }

        internal static ApprenticeshipIncentiveModel Map(this Models.ApprenticeshipIncentive entity)
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
                     )
            };
        }
    }
}
