using SFA.DAS.EmployerIncentives.Data.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public class IncentiveApplicationStatusAuditDataRepository : IIncentiveApplicationStatusAuditDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public IncentiveApplicationStatusAuditDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(IncentiveApplicationAudit incentiveApplicationAudit)
        {
            await _dbContext.AddAsync(incentiveApplicationAudit.Map());
        }

        public List<IncentiveApplicationStatusAudit> GetByApplicationApprenticeshipId(Guid incentiveApplicationApprenticeshipId)
        {
            return _dbContext.IncentiveApplicationStatusAudits.Where(x => x.IncentiveApplicationApprenticeshipId == incentiveApplicationApprenticeshipId).ToList();
        }
    }
}
