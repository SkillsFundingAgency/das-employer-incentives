using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class ChangeOfCircumstancesDataRepository : IChangeOfCircumstancesDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public ChangeOfCircumstancesDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task Save(ChangeOfCircumstance changeOfCircumstance)
        {
            await _dbContext.AddAsync(changeOfCircumstance.Map());
        }   

        public async Task<List<ChangeOfCircumstance>> GetList(Expression<Func<Models.ChangeOfCircumstance, bool>> predicate = null)
        {
            var results = await _dbContext.Set<Models.ChangeOfCircumstance>()
                .Where(predicate)
                .ToListAsync();

            return await Task.FromResult(results.Map());
        }
    }
}
