using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.Accounts;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public class IncentiveApplicationQueryRepository : IQueryRepository<IncentiveApplicationDto>
    {
        private class JoinedObject
        {
            public Models.IncentiveApplication Application { get; set; }
            public Models.Account Account { get; set; }
        }

        private readonly EmployerIncentivesDbContext _context;

        public IncentiveApplicationQueryRepository(EmployerIncentivesDbContext context)
        {
            _context = context;
        }

        public Task<IncentiveApplicationDto> Get(Expression<Func<IncentiveApplicationDto, bool>> predicate)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Join(_context.Set<Models.Account>(), app => app.AccountLegalEntityId, acc => acc.AccountLegalEntityId, (application, account) => new JoinedObject { Account = account, Application = application })
                .Select(MapToIncentiveApplicationDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<IncentiveApplicationDto>> GetList(Expression<Func<IncentiveApplicationDto, bool>> predicate = null)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Join(_context.Set<Models.Account>(), app => app.AccountLegalEntityId, acc => acc.AccountLegalEntityId, (application, account) => new JoinedObject { Account = account, Application = application })
                .Select(MapToIncentiveApplicationDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<JoinedObject, IncentiveApplicationDto>> MapToIncentiveApplicationDto()
        {
            return x => new IncentiveApplicationDto
            {
                Id = x.Application.Id,
                AccountId = x.Application.AccountId,
                AccountLegalEntityId = x.Application.AccountLegalEntityId,
                Apprenticeships = x.Application.Apprenticeships.Select(y => MapToApprenticeshipDto(y)),
                LegalEntityId = x.Account.LegalEntityId,
                SubmittedByEmail = x.Application.SubmittedByEmail,
                SubmittedByName = x.Application.SubmittedByName,
                BankDetailsRequired = MapBankDetailsRequired(x.Account.VrfCaseStatus)
            };
        }

        private static IncentiveApplicationApprenticeshipDto MapToApprenticeshipDto(Models.IncentiveApplicationApprenticeship apprenticeship)
        {
            return new IncentiveApplicationApprenticeshipDto
            {
                Id = apprenticeship.Id,
                ApprenticeshipId = apprenticeship.ApprenticeshipId,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                TotalIncentiveAmount = apprenticeship.TotalIncentiveAmount
            };
        }

        private static bool MapBankDetailsRequired(string vrfCaseStatus)
        {
            return (String.IsNullOrWhiteSpace(vrfCaseStatus) 
                || vrfCaseStatus.Equals(LegalEntityVrfCaseStatus.RejectedDataValidation, StringComparison.InvariantCultureIgnoreCase)
                || vrfCaseStatus.Equals(LegalEntityVrfCaseStatus.RejectedVer1, StringComparison.InvariantCultureIgnoreCase)
                || vrfCaseStatus.Equals(LegalEntityVrfCaseStatus.RejectedVerification, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
