using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;

namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface IIncentivePaymentProfilesService
    {
        IEnumerable<IncentivePaymentProfile> Get();
    }
}
