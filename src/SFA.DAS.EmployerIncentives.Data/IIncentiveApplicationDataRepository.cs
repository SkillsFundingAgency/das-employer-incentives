using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;

namespace SFA.DAS.EmployerIncentives.Data
{
    public interface IIncentiveApplicationDataRepository
    {
        Task Add(IncentiveApplicationModel incentiveApplication);
    }
}
