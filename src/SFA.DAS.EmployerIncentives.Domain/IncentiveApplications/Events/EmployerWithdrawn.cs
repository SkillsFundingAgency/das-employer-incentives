using SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Domain.IncentiveApplications.Events
{
    public class EmployerWithdrawn : ApplicationWithdrawn
    {
        public EmployerWithdrawn(
            long accountId,
            long accountLegalEntityId,
            ApprenticeshipModel model,
            ServiceRequest serviceRequest)
            : base(
                  IncentiveApplicationStatus.EmployerWithdrawn,
                  accountId,
                  accountLegalEntityId,
                  model,
                  serviceRequest)
        {
        }
    }
}
