using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class ReinstatedPendingPaymentAuditRepository : IReinstatedPendingPaymentAuditRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public ReinstatedPendingPaymentAuditRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(ReinstatedPendingPaymentAudit reinstatedPendingPaymentAudit)
        {
            await _dbContext.AddAsync(reinstatedPendingPaymentAudit.Map());
        }
    }
}
