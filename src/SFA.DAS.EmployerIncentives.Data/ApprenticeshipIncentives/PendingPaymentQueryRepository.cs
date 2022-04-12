using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives.Models;
using SFA.DAS.EmployerIncentives.Data.Models;
using SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives;
using PendingPayment = SFA.DAS.EmployerIncentives.DataTransferObjects.Queries.ApprenticeshipIncentives.PendingPayment;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class PendingPaymentQueryRepository : IQueryRepository<PendingPayment>
    {
        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public PendingPaymentQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<PendingPayment> Get(Expression<Func<PendingPayment, bool>> predicate)
        {
            return _context.Set<Models.PendingPayment>()
                .Select(PendingPaymentToPendingPaymentDto()).SingleOrDefaultAsync(predicate);
        }

        public Task<List<PendingPayment>> GetList(Expression<Func<PendingPayment, bool>> predicate = null)
        {
            return _context.Set<Models.PendingPayment>()
                .Select(PendingPaymentToPendingPaymentDto()).Where(predicate).ToListAsync();
        }

        private Expression<Func<Models.PendingPayment, PendingPayment>> PendingPaymentToPendingPaymentDto()
        {
            return x => new PendingPayment()
            {
                Id = x.Id,
                AccountLegalEntityId = x.AccountLegalEntityId,
                PeriodNumber = x.PeriodNumber,
                PaymentYear = x.PaymentYear,
                PaymentMadeDate = x.PaymentMadeDate,
                ApprenticeshipIncentiveId = x.ApprenticeshipIncentiveId
            };
        }
    }
}
