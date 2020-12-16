using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class EmployerWithdrawnAuditHandler : ApplicationWithdrawnAuditHandler<EmployerWithdrawn>
    {
        public EmployerWithdrawnAuditHandler(IIncentiveApplicationStatusAuditDataRepository auditRepository)
            : base(auditRepository)
        {
        }
    }
}
