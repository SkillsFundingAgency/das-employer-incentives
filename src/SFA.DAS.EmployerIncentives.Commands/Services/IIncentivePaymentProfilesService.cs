using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Commands.Services
{
    public interface IIncentivePaymentProfilesService
    {
        Task<IEnumerable<IncentivePaymentProfile>> Get();
    }
}
