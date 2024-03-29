﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.Domain.ValueObjects;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries;

namespace SFA.DAS.EmployerIncentives.Data.IncentiveApplication
{
    public class IncentiveApplicationQueryRepository : IIncentiveApplicationQueryRepository
    {
        private class JoinedObject
        {
            public Models.IncentiveApplication Application { get; set; }
            public Models.Account Account { get; set; }
        }

        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public IncentiveApplicationQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<DataTransferObjects.Queries.IncentiveApplication> Get(Expression<Func<DataTransferObjects.Queries.IncentiveApplication, bool>> predicate)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Join(_context.Set<Models.Account>(), app => app.AccountLegalEntityId, acc => acc.AccountLegalEntityId, (application, account) => new JoinedObject { Account = account, Application = application })
                .Select(MapToIncentiveApplicationDto()).FirstOrDefaultAsync(predicate);
        }

        public Task<List<DataTransferObjects.Queries.IncentiveApplication>> GetList(Expression<Func<DataTransferObjects.Queries.IncentiveApplication, bool>> predicate = null)
        {
            return _context.Set<Models.IncentiveApplication>()
                .Join(_context.Set<Models.Account>(), app => app.AccountLegalEntityId, acc => acc.AccountLegalEntityId, (application, account) => new JoinedObject { Account = account, Application = application })
                .Select(MapToIncentiveApplicationDto()).Where(predicate).ToListAsync();
        }

        private static Expression<Func<JoinedObject, DataTransferObjects.Queries.IncentiveApplication>> MapToIncentiveApplicationDto()
        {
            return x => new DataTransferObjects.Queries.IncentiveApplication
            {
                Id = x.Application.Id,
                AccountId = x.Application.AccountId,
                AccountLegalEntityId = x.Application.AccountLegalEntityId,
                Apprenticeships = x.Application.Apprenticeships
                                    .OrderBy(o => o.FirstName)
                                    .ThenBy(o => o.LastName)
                                    .ThenBy(o=> o.ULN)
                                    .Select(y => MapToApprenticeshipDto(y)),
                LegalEntityId = x.Account.LegalEntityId,
                SubmittedByEmail = x.Application.SubmittedByEmail,
                SubmittedByName = x.Application.SubmittedByName,
                BankDetailsRequired = (new VendorBankStatus(x.Account.VrfVendorId, new VendorCase(x.Account.VrfCaseId, x.Account.VrfCaseStatus, x.Account.VrfCaseStatusLastUpdatedDateTime))).BankDetailsRequired
            };
        }

        private static DataTransferObjects.Queries.IncentiveApplicationApprenticeship MapToApprenticeshipDto(Models.IncentiveApplicationApprenticeship apprenticeship)
        {
            var apprenticeshipDto = new DataTransferObjects.Queries.IncentiveApplicationApprenticeship
            {
                Id = apprenticeship.Id,
                ApprenticeshipId = apprenticeship.ApprenticeshipId,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                Uln = apprenticeship.ULN,
                PlannedStartDate = apprenticeship.PlannedStartDate,
                DateOfBirth = apprenticeship.DateOfBirth,
                EmploymentStartDate = apprenticeship.EmploymentStartDate,                
                Phase = apprenticeship.Phase,
                StartDatesAreEligible = apprenticeship.StartDatesAreEligible
            };
            
            apprenticeshipDto.TotalIncentiveAmount = Incentive.Create(apprenticeshipDto).GetTotalIncentiveAmount();

            return apprenticeshipDto;

        }
    }
}
