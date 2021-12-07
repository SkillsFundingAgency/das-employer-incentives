using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class EmploymentCheckAuditRepository : IEmploymentCheckAuditRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public EmploymentCheckAuditRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(EmploymentCheckRequestAudit employmentCheckRequestAudit)
        {
            await _dbContext.AddAsync(employmentCheckRequestAudit.Map());
        }
    }
}
