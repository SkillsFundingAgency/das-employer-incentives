using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Domain.Interfaces
{
    public interface IIncentivePaymentProfilesService
    {
        Task<IEnumerable<IncentivePaymentProfile>> Get();
    }
}
