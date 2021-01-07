using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class UlnValidationService : IUlnValidationService
    {
        private readonly EmployerIncentivesDbContext _context;

        public UlnValidationService(Lazy<EmployerIncentivesDbContext> context)
        {
            _context = context.Value;
        }

        public Task<bool> UlnAlreadyOnSubmittedIncentiveApplication(long uln)
        {
            var query =
                from apprentices in _context.ApplicationApprenticeships
                join applications in _context.Applications on apprentices.IncentiveApplicationId equals applications.Id
                where apprentices.ULN == uln && applications.Status == IncentiveApplicationStatus.Submitted
                && !apprentices.WithdrawnByEmployer
                select new { apprentices, applications };

            return query.AnyAsync();
        }
    }
}