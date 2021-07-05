using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.EmployerIncentives.Data.ApprenticeshipIncentives
{
    public interface IPaymentDataRepository
    {
        Task RecordPaymentsSent(List<Guid> paymentIds, long accountLegalEntityId, DateTime paidDate);
        Task RecordClawbacksSent(List<Guid> clawbackIds, long accountLegalEntityId, DateTime clawbackDate);
    }
}
