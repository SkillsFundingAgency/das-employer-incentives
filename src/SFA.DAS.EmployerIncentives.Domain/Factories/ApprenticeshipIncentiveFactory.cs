
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public class ApprenticeshipIncentiveFactory : IApprenticeshipIncentiveFactory
    {
        public ApprenticeshipIncentive CreateNew(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime plannedStartDate, DateTime submittedDate, string submittedByEmail)
        {
            return ApprenticeshipIncentive.New(id, applicationApprenticeshipId, account, apprenticeship, plannedStartDate, false, submittedDate, submittedByEmail);
        }

        public ApprenticeshipIncentive GetExisting(Guid id, ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(id, model);
        }
    }
}
