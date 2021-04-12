
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public class ApprenticeshipIncentiveFactory : IApprenticeshipIncentiveFactory
    {
        public ApprenticeshipIncentive CreateNew(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime plannedStartDate, DateTime submittedDate, string submittedByEmail, int? minimumAgreementVersion)
        {
            return ApprenticeshipIncentive.New(id, applicationApprenticeshipId, account, apprenticeship, plannedStartDate, submittedDate, submittedByEmail, minimumAgreementVersion);
        }

        public ApprenticeshipIncentive GetExisting(Guid id, ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(id, model);
        }
    }
}
