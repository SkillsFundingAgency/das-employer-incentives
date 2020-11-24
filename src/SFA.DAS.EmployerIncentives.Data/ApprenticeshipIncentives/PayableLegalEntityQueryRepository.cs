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

        public Task<List<PaymentDto>> GetPaymentsToSendForAccountLegalEntity(long accountLegalEntity)
        {
            var payments = _context.Set<Payment>().Where(p => !p.PaidDate.HasValue && p.AccountLegalEntityId == accountLegalEntity)
                .Join(_context.Set<Models.ApprenticeshipIncentive>(), p => p.ApprenticeshipIncentiveId, ai => ai.Id,
                    (p, ai) => new {ApprenticeshipIncentive = ai, Payment = p})
                .Join(_context.Set<Data.Models.Account>(), ap => ap.Payment.AccountLegalEntityId, a => a.AccountLegalEntityId, (ap, a) =>
                    new { ap.ApprenticeshipIncentive, ap.Payment, Account = a})
                .Join(_context.Set<PendingPayment>(), ap => ap.Payment.PendingPaymentId, pp => pp.Id, (ap, pp) =>
                    new
                    {
                        ap.ApprenticeshipIncentive, 
                        ap.Payment,
                        ap.Account, 
                        PendingPayment = pp
                    })
                .Select(x =>
                    new PaymentDto
                    {
                        PaymentId = x.Payment.Id,
                        ApprenticeshipIncentiveId = x.ApprenticeshipIncentive.Id,
                        AccountLegalEntityId = x.Account.AccountLegalEntityId,
                        VendorId = x.Account.VrfVendorId,
                        DueDate = x.PendingPayment.DueDate,
                        SubnominalCode = x.Payment.SubnominalCode,
                        Amount = x.Payment.Amount,
                        PaymentSequence = "first", // TODO needs to be updated to either first or Second
                        ULN = x.ApprenticeshipIncentive.ULN,
                        HashedLegalEntityId = x.Account.HashedLegalEntityId
                    });

            return payments.ToListAsync();
        }
    }
}
