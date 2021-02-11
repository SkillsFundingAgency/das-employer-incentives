using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public interface IIncentiveApplicationStatusAuditDataRepository
    {
        Task Add(IncentiveApplicationAudit incentiveApplicationAudit);        
    }
}
