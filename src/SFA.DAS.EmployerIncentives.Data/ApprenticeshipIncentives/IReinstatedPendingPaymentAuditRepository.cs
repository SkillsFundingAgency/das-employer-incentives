using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IReinstatedPendingPaymentAuditRepository
    {
        Task Add(ReinstatedPendingPaymentAudit reinstatedPendingPaymentAudit);
    }
}
