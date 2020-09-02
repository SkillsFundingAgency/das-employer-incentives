using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.EmployerIncentives.Functions.DomainMessageHandlers
{
    public static class QueueNames
    {
        public const string LegalEntityAdded = "SFA.DAS.EmployerIncentives.LegalEntityAdded";
        public const string RemovedLegalEntity = "SFA.DAS.EmployerIncentives.LegalEntityRemoved";
        public const string RefreshLegalEntities = "SFA.DAS.EmployerIncentives.RefreshLegalEntities";
        public const string RefreshLegalEntity = "SFA.DAS.EmployerIncentives.RefreshLegalEntity";
        public const string AgreementSigned = "SFA.DAS.EmployerIncentives.AgreementSigned";

        public const string ApplicationCalculateClaim = "SFA.DAS.EmployerIncentives.ApplicationCalculateClaim";
    }
}
