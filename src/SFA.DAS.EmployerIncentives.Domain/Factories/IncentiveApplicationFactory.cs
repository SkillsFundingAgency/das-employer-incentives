using System;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public class IncentiveApplicationFactory : IIncentiveApplicationFactory
    {
        public IncentiveApplication CreateNew(Guid id, long accountId, long accountLegalEntityId)
        {
            return IncentiveApplication.New(id, accountId, accountLegalEntityId);
        }

        public Apprenticeship CreateNewApprenticeship(int apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate, ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval)
        {
            return new Apprenticeship(Guid.NewGuid(), apprenticeshipId, firstName, lastName, dateOfBirth, uln, plannedStartDate, apprenticeshipEmployerTypeOnApproval);
        }

        public IncentiveApplication GetExisting(Guid id, IncentiveApplicationModel model)
        {
            return IncentiveApplication.Get(id, model);
        }
    }
}
