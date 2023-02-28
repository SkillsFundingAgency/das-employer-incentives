using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;

namespace SFA.DAS.EmployerIncentives.Data.Account
{
    public class VendorBlockAuditRepository : IVendorBlockAuditRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public VendorBlockAuditRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(VendorBlockRequestAudit vendorBlockRequestAudit)
        {
            await _dbContext.VendorBlockAudits.AddAsync(vendorBlockRequestAudit.Map());
        }
    }
}
