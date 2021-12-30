using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.Services.EmploymentCheckApi
{
    public interface IEmploymentCheckService
    {
        public Task<Guid> RegisterEmploymentCheck(EmploymentCheck employmentCheck, Domain.ApprenticeshipIncentives.ApprenticeshipIncentive apprenticeshipIncentive);
    }
}
