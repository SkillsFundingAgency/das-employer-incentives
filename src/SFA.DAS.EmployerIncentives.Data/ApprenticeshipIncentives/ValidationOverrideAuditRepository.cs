using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using System;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class ValidationOverrideAuditRepository : IValidationOverrideAuditRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public ValidationOverrideAuditRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Add(ValidationOverrideStepAudit validationOverrideStepAudit)
        {
            await _dbContext.AddAsync(validationOverrideStepAudit.Map());
        }

        public async Task Delete(Guid id)
        {            
            var existingAuditStep = await _dbContext.ValidationOverrideAudits.FirstOrDefaultAsync(x => x.Id == id);
           
            if (existingAuditStep != null)
            {
                existingAuditStep.DeletedDateTime = DateTime.Now;
                _dbContext.Entry(existingAuditStep).CurrentValues.SetValues(existingAuditStep);
            }
        }
    }
}
