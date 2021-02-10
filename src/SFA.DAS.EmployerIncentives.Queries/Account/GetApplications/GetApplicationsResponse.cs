using System;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Enums;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetApplications
{
    public class GetApplicationsResponse
    {
        public IEnumerable<ApprenticeApplicationDto> ApprenticeApplications { get; set; }
        public BankDetailsStatus BankDetailsStatus { get; set; }
        public Guid? FirstSubmittedApplicationId { get; set; }
    }
}
