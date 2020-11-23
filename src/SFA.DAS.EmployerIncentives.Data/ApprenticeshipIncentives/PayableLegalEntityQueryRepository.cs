using System;
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
        private class JoinedObject
        {
            public Models.ApprenticeshipIncentive ApprenticeshipIncentive { get; set; }
            public Models.Payment Payment { get; set; }
        }


        private Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public PayableLegalEntityQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<List<PayableLegalEntityDto>> GetList(short collectionPeriodYear, byte collectionPeriodNumber)
        {
            var accountLegalEntities = _context.Set<PendingPayment>().Where(x => !x.PaymentMadeDate.HasValue && (x.PaymentYear < collectionPeriodYear || (x.PaymentYear == collectionPeriodYear && x.PeriodNumber <= collectionPeriodNumber)))
                .Select(x => new { x.AccountLegalEntityId, x.AccountId }).Distinct();

            return accountLegalEntities.Select(x=> new PayableLegalEntityDto {AccountLegalEntityId = x.AccountLegalEntityId, AccountId = x.AccountId }).ToListAsync();
        }

        public Task<List<PaymentDto>> GetPaymentsToSendForAccountLegalEntity(long accountLegalEntity)
        {
            var payments = _context.Set<Payment>().Where(p => !p.PaidDate.HasValue )
                .Join(_context.Set<Models.ApprenticeshipIncentive>(), p => p.ApprenticeshipIncentiveId, ai => ai.Id, (p, ai) => 
                    new { ApprenticeshipIncentive = ai, Payment = p})
                .Join(_context.Set<Data.Models.Account>(), ap => ap.Payment.AccountId, a => a.Id, (ap, a) =>
                    new { ApprenticeshipIncentive = ap.ApprenticeshipIncentive, Payment = ap.Payment, Account = a })
                .Select(x => 
                    new PaymentDto { PaymentId = x.Payment.Id, ApprenticeshipIncentiveId = x.ApprenticeshipIncentive.IncentiveApplicationApprenticeshipId, VendorId = x.Account.VrfVendorId});

            return payments.ToListAsync();
        }
    }
}
