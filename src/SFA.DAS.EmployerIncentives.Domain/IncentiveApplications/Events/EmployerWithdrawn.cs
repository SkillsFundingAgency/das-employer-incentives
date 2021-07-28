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
            string legalEntity,
            string emailAddress,
            ApprenticeshipModel model,
            ServiceRequest serviceRequest)
            : base(
                  IncentiveApplicationStatus.EmployerWithdrawn,
                  accountId,
                  accountLegalEntityId,
                  model,
                  serviceRequest)
        {
            LegalEntity = legalEntity;
            EmailAddress = emailAddress;
        }

        public string LegalEntity { get; }
        public string EmailAddress { get; }
    }
}
