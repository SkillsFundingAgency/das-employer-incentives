using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public interface IApprenticeshipIncentiveFactory
    {
        ApprenticeshipIncentive CreateNew(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime plannedStartDate, DateTime submittedDate, string submittedByEmail);
        ApprenticeshipIncentive GetExisting(Guid id, ApprenticeshipIncentiveModel model);
    }
}
