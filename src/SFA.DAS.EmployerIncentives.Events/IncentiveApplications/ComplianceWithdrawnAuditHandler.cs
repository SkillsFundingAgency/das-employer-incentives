using SFA.DAS.EmployerIncentives.Data.IncentiveApplication;
using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events;

namespace SFA.DAS.EmployerIncentives.Events.IncentiveApplications
{
    public class ComplianceWithdrawnAuditHandler : ApplicationWithdrawnAuditHandler<ComplianceWithdrawn>
    {
        public ComplianceWithdrawnAuditHandler(IIncentiveApplicationStatusAuditDataRepository auditRepository)
            : base(auditRepository)
        {
        }
    }
}
