using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public interface IEmploymentCheckService
    {
        public Task<Guid> RegisterEmploymentCheck(EmploymentCheck employmentCheck);
    }
}
