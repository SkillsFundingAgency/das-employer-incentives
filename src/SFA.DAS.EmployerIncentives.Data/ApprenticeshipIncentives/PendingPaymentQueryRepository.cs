using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class PendingPaymentQueryRepository : IQueryRepository<PendingPaymentDto>
    {
        private readonly EmployerIncentivesDbContext _context;

        public PendingPaymentQueryRepository(EmployerIncentivesDbContext context)
        {
            _context = context;
        }

        public Task<PendingPaymentDto> Get(Expression<Func<PendingPaymentDto, bool>> predicate)
        {
            return _context.Set<PendingPayment>()
                .Select(PendingPaymentToPendingPaymentDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<PendingPaymentDto>> GetList(Expression<Func<PendingPaymentDto, bool>> predicate = null)
        {
            return _context.Set<PendingPayment>()
                .Select(PendingPaymentToPendingPaymentDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<PendingPayment, PendingPaymentDto>> PendingPaymentToPendingPaymentDto()
        {
            return x => new PendingPaymentDto()
            {
                Id = x.Id,
                AccountLegalEntityId = x.AccountLegalEntityId,
                PaymentPeriod = x.PaymentPeriod,
                PaymentYear = x.PaymentYear,
                PaymentMadeDate = x.PaymentMadeDate,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId
            };
        }
    }
}
