using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public class IncentiveApplicationQueryRepository : IQueryRepository<IncentiveApplicationDto>
    {
        private readonly EmployerIncentivesDbContext _context;

        public IncentiveApplicationQueryRepository(EmployerIncentivesDbContext context)
        {
            _context = context;
        }

        public Task<IncentiveApplicationDto> Get(Expression<Func<IncentiveApplicationDto, bool>> predicate)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Select(MapToIncentiveApplicationDto()).SingleAsync(predicate);
        }

        public Task<List<IncentiveApplicationDto>> GetList(Expression<Func<IncentiveApplicationDto, bool>> predicate = null)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Select(MapToIncentiveApplicationDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<Models.IncentiveApplication, IncentiveApplicationDto>> MapToIncentiveApplicationDto()
        {
            return x => new IncentiveApplicationDto
            {
                Id = x.Id,
                AccountId = x.AccountId,
                Apprenticeships = x.Apprenticeships.Select(y => MapToApprenticeshipDto(y))
            };
        }

        private static IncentiveApplicationApprenticeshipDto MapToApprenticeshipDto(IncentiveApplicationApprenticeship apprenticeship)
        {
            return new IncentiveApplicationApprenticeshipDto
            {
                ApprenticeshipId = apprenticeship.ApprenticeshipId,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                TotalIncentiveAmount = apprenticeship.TotalIncentiveAmount
            };
        }
    }
}
