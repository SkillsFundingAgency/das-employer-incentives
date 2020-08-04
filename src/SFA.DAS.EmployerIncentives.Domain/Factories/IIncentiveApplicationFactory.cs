using System;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public interface IIncentiveApplicationFactory
    {
        IncentiveApplication CreateNew(Guid id, long accountId, long accountLegalEntityId);
        Apprenticeship CreateNewApprenticeship(Guid id, int apprenticeshipId, string firstName, string lastName, DateTime dateOfBirth, long uln, DateTime plannedStartDate, ApprenticeshipEmployerType apprenticeshipEmployerTypeOnApproval);
        IncentiveApplication GetExisting(Guid id, IncentiveApplicationModel model);
    }
}
