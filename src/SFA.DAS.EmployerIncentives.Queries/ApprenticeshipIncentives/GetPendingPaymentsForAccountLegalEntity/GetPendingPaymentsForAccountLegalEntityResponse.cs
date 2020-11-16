using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Queries.ApprenticeshipIncentives.GetPendingPaymentsForAccountLegalEntity
{
    public class GetPendingPaymentsForAccountLegalEntityResponse
    {
        public List<PendingPaymentDto> PendingPayments { get; }

        public GetPendingPaymentsForAccountLegalEntityResponse(List<PendingPaymentDto> pendingPayments)
        {
            PendingPayments = pendingPayments;
        }
    }
}
