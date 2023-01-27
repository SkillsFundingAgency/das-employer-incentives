using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IRevertedPaymentAuditRepository
    {
        Task Add(RevertedPaymentAudit revertedPaymentAudit);
    }
}
