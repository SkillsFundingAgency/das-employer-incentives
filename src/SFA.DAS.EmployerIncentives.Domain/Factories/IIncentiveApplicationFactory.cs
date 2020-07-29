using System;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications;

namespace SFA.DAS.EmployerIncentives.Domain.Factories
{
    public interface IIncentiveApplicationFactory
    {
        IncentiveApplication CreateNew(Guid id, long accountId, long accountLegalEntityId);
    }
}
