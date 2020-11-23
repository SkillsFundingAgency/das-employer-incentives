using System.Collections.Generic;
using SFA.DAS.EmployerIncentives.Abstractions.DTOs.Queries.ApprenticeshipIncentives;

namespace SFA.DAS.EmployerIncentives.Commands.Services.BusinessCentralApi
{
    public class PaymentsSuccessfullySent
    {
        public PaymentsSuccessfullySent(List<PaymentDto> paymentsSent, bool allPaymentsSent)
        {
            PaymentsSent = paymentsSent;
            AllPaymentsSent = allPaymentsSent;
        }
        public List<PaymentDto> PaymentsSent { get; }
        public bool AllPaymentsSent { get; }
    }
}