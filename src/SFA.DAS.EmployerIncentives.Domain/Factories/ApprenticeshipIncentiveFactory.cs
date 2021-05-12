
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public class ApprenticeshipIncentiveFactory : IApprenticeshipIncentiveFactory
    {
        public ApprenticeshipIncentive CreateNew(Guid id, Guid applicationApprenticeshipId, Account account, Apprenticeship apprenticeship, DateTime plannedStartDate, DateTime submittedDate, string submittedByEmail, AgreementVersion agreementVersion, IncentivePhase phase)
        {
            return ApprenticeshipIncentive.New(id, applicationApprenticeshipId, account, apprenticeship, plannedStartDate, submittedDate, submittedByEmail, agreementVersion, phase);
        }

        public ApprenticeshipIncentive GetExisting(Guid id, ApprenticeshipIncentiveModel model)
        {
            return ApprenticeshipIncentive.Get(id, model);
        }
    }
}
