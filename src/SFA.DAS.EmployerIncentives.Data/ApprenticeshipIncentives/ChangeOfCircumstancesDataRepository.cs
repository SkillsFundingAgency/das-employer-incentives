using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ApprenticeshipIncentives.ValueTypes;
using System;
using System.Threading.Tasks;

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
    }
}
