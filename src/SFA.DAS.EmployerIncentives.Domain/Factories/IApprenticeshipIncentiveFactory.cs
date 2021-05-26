using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public interface IApprenticeshipIncentiveFactory
    {
        ApprenticeshipIncentive CreateNew(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime plannedStartDate, DateTime submittedDate, string submittedByEmail, AgreementVersion minimumAgreementVersion, IncentivePhase phase);
        ApprenticeshipIncentive GetExisting(Guid id, ApprenticeshipIncentiveModel model);
    }
}
