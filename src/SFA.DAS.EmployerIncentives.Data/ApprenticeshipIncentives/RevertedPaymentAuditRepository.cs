using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class RevertedPaymentAuditRepository : IRevertedPaymentAuditRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public RevertedPaymentAuditRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(RevertedPaymentAudit revertedPaymentAudit)
        {
            await _dbContext.AddAsync(revertedPaymentAudit.Map());
        }
    }
}
