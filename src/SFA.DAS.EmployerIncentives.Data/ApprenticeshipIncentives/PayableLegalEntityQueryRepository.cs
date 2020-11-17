﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class PayableLegalEntityQueryRepository : IPayableLegalEntityQueryRepository
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public PayableLegalEntityQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<List<PayableLegalEntityDto>> GetList(short collectionPeriodYear, byte collectionPeriodMonth)
        {
            var accountLegalEntities = _context.Set<PendingPayment>().Where(x => !x.PaymentMadeDate.HasValue && (x.PaymentYear < collectionPeriodYear || (x.PaymentYear == collectionPeriodYear && x.PeriodNumber <= collectionPeriodMonth)))
                .Select(x => new { x.AccountLegalEntityId, x.AccountId }).Distinct();

            return accountLegalEntities.Select(x=> new PayableLegalEntityDto {AccountLegalEntityId = x.AccountLegalEntityId, AccountId = x.AccountId }).ToListAsync();
        }
    }
}
