using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IPaymentDataRepository
    {
        Task UpdatePaidDates(List<Guid> paymentIds, DateTime paidDate);
        Task UpdateClawbackDates(List<Guid> clawbackIds, DateTime clawbackDate);
    }
}
