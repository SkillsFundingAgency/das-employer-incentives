using Microsoft.EntityFrameworkCore;
using SFA.DAS.EmployerIncentives.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public class PaymentDataRepository : IPaymentDataRepository
    {
        private readonly EmployerIncentivesDbContext _dbContext;
        
        public PaymentDataRepository(Lazy<EmployerIncentivesDbContext> context)
        {
            _dbContext = context.Value;
        }

        public async Task RecordPaymentsSent(List<Guid> paymentIds, long accountLegalEntityId, DateTime paidDate)
        {
            var account = _dbContext.Accounts.FirstOrDefault(x => x.AccountLegalEntityId == accountLegalEntityId);
            if (account == null)
            {
                return;
            }

            var payments = await _dbContext.Payments.Where(x => paymentIds.Contains(x.Id) && !x.PaidDate.HasValue).ToListAsync();
            foreach (var paymentId in paymentIds)
            {
                var payment = payments.SingleOrDefault(p => p.Id == paymentId);
                if (payment != null)
                {
                    payment.PaidDate ??= paidDate;
                    payment.VrfVendorId = account.VrfVendorId;
                }
            }
        }

        public async Task RecordClawbacksSent(List<Guid> clawbackIds, long accountLegalEntityId, DateTime clawbackDate)
        {
            var account = _dbContext.Accounts.FirstOrDefault(x => x.AccountLegalEntityId == accountLegalEntityId);
            if (account == null)
            {
                return;
            }

            var clawbacks = await _dbContext.ClawbackPayments.Where(x => clawbackIds.Contains(x.Id) && !x.DateClawbackSent.HasValue).ToListAsync();

            foreach (var clawbackId in clawbackIds)
            {
                var clawback = clawbacks.SingleOrDefault(p => p.Id == clawbackId);
                if (clawback != null)
                {
                    clawback.DateClawbackSent ??= clawbackDate;
                    clawback.VrfVendorId = account.VrfVendorId;
                }
            }
        }
    }
}
