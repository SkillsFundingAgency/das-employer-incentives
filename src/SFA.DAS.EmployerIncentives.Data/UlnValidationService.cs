﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Enums;

namespace SFA.DAS.EmployerIncentives.Data
{
    public class UlnValidationService : IUlnValidationService
    {
        private readonly EmployerIncentivesDbContext _context;

        public UlnValidationService(EmployerIncentivesDbContext context)
        {
            _context = context;
        }

        public Task<bool> UlnAlreadyOnSubmittedIncentiveApplication(long uln)
        {
            var query =
                from apprentices in _context.ApplicationApprenticeships
                join applications in _context.Applications on apprentices.IncentiveApplicationId equals applications.Id
                where apprentices.ULN == uln && applications.Status == IncentiveApplicationStatus.Submitted
                select new {apprentices, applications};

            return query.AnyAsync();
        }
    }
}