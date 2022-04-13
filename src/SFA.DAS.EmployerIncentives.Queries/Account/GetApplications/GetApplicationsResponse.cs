using System;
using SFA.DAS.EmployerIncentives.Enums;
using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetApplications
{
    public class GetApplicationsResponse
    {
        public IEnumerable<ApprenticeApplication> ApprenticeApplications { get; set; }
        public BankDetailsStatus BankDetailsStatus { get; set; }
        public Guid? FirstSubmittedApplicationId { get; set; }
    }
}
