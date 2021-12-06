using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IApprenticeshipIncentiveAuditRepository
    {
        Task Add(EmploymentCheckRequestAudit employmentCheckRequestAudit);
    }
}
