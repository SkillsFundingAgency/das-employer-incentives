using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using System.Collections.Generic;

namespace SFA.DAS.EmployerIncentives.Queries.Account.GetApplications
{
    public class GetApplicationsResponse
    {
        public IEnumerable<ApprenticeApplicationDto> ApprenticeApplications { get; set; }
    }
}
