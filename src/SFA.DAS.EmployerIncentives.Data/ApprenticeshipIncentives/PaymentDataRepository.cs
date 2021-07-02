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

        public async Task UpdatePaidDates(List<Guid> paymentIds, DateTime paidDate)
        {
            var payments = await _dbContext.Payments.Where(x => paymentIds.Contains(x.Id) && !x.PaidDate.HasValue).ToListAsync();
            foreach (var payment in payments)
            {
                payment.PaidDate = paidDate;
            }
        }

        public async Task UpdateClawbackDates(List<Guid> clawbackIds, DateTime clawbackDate)
        {
            var clawbacks = await _dbContext.ClawbackPayments.Where(x => clawbackIds.Contains(x.Id) && !x.DateClawbackSent.HasValue).ToListAsync();

            foreach (var clawback in clawbacks)
            {
                clawback.DateClawbackSent = clawbackDate;
            }
        }
    }
}
