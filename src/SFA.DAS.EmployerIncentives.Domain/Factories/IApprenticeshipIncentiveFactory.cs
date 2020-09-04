using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public interface IApprenticeshipIncentiveFactory
    {
        ApprenticeshipIncentive CreateNew(Guid id, Account account, Apprenticeship apprenticeship);
        ApprenticeshipIncentive GetExisting(Guid id, ApprenticeshipIncentiveModel model);
    }
}
