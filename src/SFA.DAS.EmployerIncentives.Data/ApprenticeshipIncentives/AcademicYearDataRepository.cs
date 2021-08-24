using Microsoft.EntityFrameworkCore;
using System;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Map;
using SFA.DAS.EmployerIncentives.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class AcademicYearDataRepository : IAcademicYearDataRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _dbContext => _lazyContext.Value;

        public AcademicYearDataRepository(Lazy<EmployerIncentivesDbContext> dbContext)
        {
            _lazyContext = dbContext;
        }

        public async Task<IEnumerable<Domain.ValueObjects.AcademicYear>> GetAll()
        {
            var academicYears = await _dbContext.Set<Models.AcademicYear>().ToListAsync();
            if (academicYears.Count > 0)
            {
                return academicYears.Map();
            }
            return null;
        }
    }
}
