
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public class ApprenticeshipIncentiveFactory : IApprenticeshipIncentiveFactory
    {
        public ApprenticeshipIncentive CreateNew(Guid id, Account account, Apprenticeship apprenticeship)
        {
            return ApprenticeshipIncentive.New(id, account, apprenticeship);
        }

        public ApprenticeshipIncentive GetExisting(Guid id, ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(id, model);
        }
    }
}
