using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface IIncentivePaymentProfilesService
    {
        Task<IncentivesConfiguration> Get();
    }
}
