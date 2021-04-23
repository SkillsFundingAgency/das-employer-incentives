using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface IIncentivePaymentProfilesService
    {
        Task<IncentiveProfiles> Get();
        Task<IncentiveProfiles> Get(IncentivePhase phase);
    }
}
