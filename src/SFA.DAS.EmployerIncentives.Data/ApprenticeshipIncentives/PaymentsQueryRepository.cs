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
    public class PaymentsQueryRepository : IPaymentsQueryRepository
    {
        private readonly Lazy<EmployerIncentivesDbContext> _lazyContext;
        private EmployerIncentivesDbContext _context => _lazyContext.Value;

        public PaymentsQueryRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _lazyContext = context;
        }

        public Task<List<PayableLegalEntityDto>> GetPayableLegalEntities(short collectionPeriodYear, byte collectionPeriodNumber)
        {
            var accountLegalEntities = _context.Set<PendingPayment>().Where(x => !x.PaymentMadeDate.HasValue && (x.PaymentYear < collectionPeriodYear || (x.PaymentYear == collectionPeriodYear && x.PeriodNumber <= collectionPeriodNumber)))
                .Select(x => new { x.AccountLegalEntityId, x.AccountId })
                .Union(_context.Set<Payment>().Where(x => !x.PaidDate.HasValue).Select(x => new { x.AccountLegalEntityId, x.AccountId }))
                .Distinct();

            return accountLegalEntities.Select(x=> new PayableLegalEntityDto {AccountLegalEntityId = x.AccountLegalEntityId, AccountId = x.AccountId }).ToListAsync();
        }

        public Task<List<PaymentDto>> GetUnpaidPayments(long accountLegalEntity)
        {
            var payments = _context.Set<Payment>().Where(p => !p.PaidDate.HasValue && p.AccountLegalEntityId == accountLegalEntity)
                .Join(_context.Set<ApprenticeshipIncentive>(), p => p.ApprenticeshipIncentiveId, ai => ai.Id,
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
                        EarningType = x.PendingPayment.EarningType, 
                        ULN = x.ApprenticeshipIncentive.ULN,
                        HashedLegalEntityId = x.Account.HashedLegalEntityId
                    });

            return payments.ToListAsync();
        }

        public Task<List<ClawbackLegalEntityDto>> GetClawbackLegalEntities(short collectionPeriodYear, byte collectionPeriodNumber, bool isSent = false)
        {
            var accountLegalEntities = _context.Set<ClawbackPayment>()
                .Where(x => (isSent ? x.DateClawbackSent.HasValue : !x.DateClawbackSent.HasValue) 
                    && (x.CollectionPeriodYear < collectionPeriodYear || (x.CollectionPeriodYear == collectionPeriodYear && x.CollectionPeriod <= collectionPeriodNumber)))
                .Select(x => new { x.AccountLegalEntityId, x.AccountId })
                .Distinct();

            return accountLegalEntities.Select(x => new ClawbackLegalEntityDto { AccountLegalEntityId = x.AccountLegalEntityId, AccountId = x.AccountId, IsSent = isSent }).ToListAsync();
        }
    }
}
